using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;
using DMS.Core.Models;
using Microsoft.Extensions.Hosting;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using Microsoft.Extensions.Logging;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;

namespace DMS.Infrastructure.Services;

/// <summary>
/// MQTT后台服务，继承自BackgroundService，用于在后台管理MQTT连接和数据发布。
/// </summary>
public class MqttBackgroundService : BackgroundService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly ILogger<MqttBackgroundService> _logger;

    private readonly ConcurrentDictionary<int, IMqttClient> _mqttClients;
    private readonly ConcurrentDictionary<int, MqttServer> _mqttConfigDic;
    private readonly ConcurrentDictionary<int, int> _reconnectAttempts;

    private readonly SemaphoreSlim _reloadSemaphore = new(0);
    private readonly Channel<VariableMqtt> _messageChannel;

    /// <summary>
    /// 构造函数，注入DataServices。
    /// </summary>
    public MqttBackgroundService(IRepositoryManager repositoryManager, ILogger<MqttBackgroundService> logger)
    {
        _repositoryManager = repositoryManager;
        _logger = logger;
        _mqttClients = new ConcurrentDictionary<int, IMqttClient>();
        _mqttConfigDic = new ConcurrentDictionary<int, MqttServer>();
        _reconnectAttempts = new ConcurrentDictionary<int, int>();
        _messageChannel = Channel.CreateUnbounded<VariableMqtt>();

        // _deviceDataService.OnMqttListChanged += HandleMqttListChanged;
    }

    /// <summary>
    /// 将待发送的变量数据异步推入队列。
    /// </summary>
    /// <param name="data">包含MQTT别名和变量数据的对象。</param>
    public async Task SendVariableAsync(VariableMqtt data)
    {
        await _messageChannel.Writer.WriteAsync(data);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Mqtt后台服务正在启动。");
        _reloadSemaphore.Release();

        var processQueueTask = ProcessMessageQueueAsync(stoppingToken);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _reloadSemaphore.WaitAsync(stoppingToken);

                if (stoppingToken.IsCancellationRequested) break;

                // if (_deviceDataService.Mqtts == null || _deviceDataService.Mqtts.Count == 0)
                // {
                //     _logger.LogInformation("没有可用的Mqtt配置，等待Mqtt列表更新...");
                //     continue;
                // }

                if (!LoadMqttConfigurations())
                {
                    _logger.LogInformation("加载Mqtt配置过程中发生了错误，停止后面的操作。");
                    continue;
                }

                await ConnectMqttList(stoppingToken);
                _logger.LogInformation("Mqtt后台服务已启动。");

                while (!stoppingToken.IsCancellationRequested && _reloadSemaphore.CurrentCount == 0)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Mqtt后台服务正在停止。");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Mqtt后台服务运行中发生了错误:{e.Message}");
        }
        finally
        {
            _messageChannel.Writer.Complete();
            await processQueueTask; // 等待消息队列处理完成
            await DisconnectAll(stoppingToken);
            // _deviceDataService.OnMqttListChanged -= HandleMqttListChanged;
            _logger.LogInformation("Mqtt后台服务已停止。");
        }
    }

    private async Task ProcessMessageQueueAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MQTT消息发送队列处理器已启动。");
        var batch = new List<VariableMqtt>();
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // 等待信号：要么是新消息到达，要么是1秒定时器触发
                await Task.WhenAny(
                    _messageChannel.Reader.WaitToReadAsync(stoppingToken).AsTask(),
                    timer.WaitForNextTickAsync(stoppingToken).AsTask()
                );

                // 尽可能多地读取消息，直到达到批次上限
                while (batch.Count < 50 && _messageChannel.Reader.TryRead(out var message))
                {
                    batch.Add(message);
                }

                if (batch.Any())
                {
                    await SendBatchAsync(batch, stoppingToken);
                    batch.Clear();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("MQTT消息发送队列处理器已停止。");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"处理MQTT消息队列时发生错误: {ex.Message}");
                await Task.Delay(5000, stoppingToken); // 发生未知错误时，延迟一段时间再重试
            }
        }
    }

    private async Task SendBatchAsync(List<VariableMqtt> batch, CancellationToken stoppingToken)
    {
        _logger.LogInformation($"准备发送一批 {batch.Count} 条MQTT消息。");
        // 按MQTT服务器ID进行分组
        var groupedByMqtt = batch.GroupBy(vm => vm.Mqtt.Id);

        foreach (var group in groupedByMqtt)
        {
            var mqttId = group.Key;
            if (!_mqttClients.TryGetValue(mqttId, out var client) || !client.IsConnected)
            {
                _logger.LogWarning($"MQTT客户端 (ID: {mqttId}) 未连接或不存在，跳过 {group.Count()} 条消息。");
                continue;
            }

            var messages = group.Select(vm => new MqttApplicationMessageBuilder()
                                              .WithTopic(vm.Mqtt.PublishTopic)
                                              .WithPayload(vm.Variable?.DataValue?.ToString() ?? string.Empty)
                                              .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtMostOnce)
                                              .Build())
                                .ToList();
            try
            {
                foreach (var message in messages)
                {
                    await client.PublishAsync(message, stoppingToken);
                }
                _logger.LogInformation($"成功向MQTT客户端 (ID: {mqttId}) 发送 {messages.Count} 条消息。");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"向MQTT客户端 (ID: {mqttId}) 批量发送消息时发生错误: {ex.Message}");
            }
        }
    }

    private async Task DisconnectAll(CancellationToken stoppingToken)
    {
        var disconnectTasks = _mqttClients.Values.Select(client => client.DisconnectAsync(new MqttClientDisconnectOptions(), stoppingToken));
        await Task.WhenAll(disconnectTasks);
        _mqttClients.Clear();
    }

    private bool LoadMqttConfigurations()
    {
        try
        {
            _logger.LogInformation("开始加载Mqtt配置文件...");
            _mqttConfigDic.Clear();
            // var mqttConfigList = _deviceDataService.Mqtts.Where(m => m.IsActive).ToList();
            //
            // foreach (var mqtt in mqttConfigList)
            // {
            //     // mqtt.OnMqttIsActiveChanged += OnMqttIsActiveChangedHandler; // 移除此行，因为MqttServer没有这个事件
            //     _mqttConfigDic.TryAdd(mqtt.Id, mqtt);
            //     // mqtt.ConnectMessage = "配置加载成功."; // 移除此行，因为MqttServer没有这个属性
            // }
            //
            // _logger.LogInformation($"Mqtt配置文件加载成功，开启的Mqtt客户端：{mqttConfigList.Count}个。");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Mqtt后台服务在加载配置的过程中发生了错误:{e.Message}");
            return false;
        }
    }

    // private async void OnMqttIsActiveChangedHandler(MqttServer mqtt) // 移除此方法，因为MqttServer没有这个事件
    // {
    //     try
    //     {
    //         if (mqtt.IsActive)
    //         {
    //             await ConnectMqtt(mqtt, CancellationToken.None);
    //         }
    //         else
    //         {
    //             if (_mqttClients.TryRemove(mqtt.Id, out var client) && client.IsConnected)
    //             {
    //                 await client.DisconnectAsync();
    //                 _logger.LogInformation($"{mqtt.Name}的客户端，与服务器断开连接.");
    //             }
    //             mqtt.IsConnected = false;
    //             mqtt.ConnectMessage = "已断开连接.";
    //         }
    //
    //         await _repositoryManager.MqttServers.UpdateAsync(mqtt);
    //         _logger.LogInformation($"Mqtt客户端：{mqtt.Name},激活状态修改成功。");
    //     }
    //     catch (Exception e)
    //     {
    //         _logger.LogError(e, $"{mqtt.Name}客户端，开启或关闭的过程中发生了错误:{e.Message}");
    //     }
    // }

    private async Task ConnectMqttList(CancellationToken stoppingToken)
    {
        var connectTasks = _mqttConfigDic.Values.Select(mqtt => ConnectMqtt(mqtt, stoppingToken));
        await Task.WhenAll(connectTasks);
    }

    private async Task ConnectMqtt(MqttServer mqtt, CancellationToken stoppingToken)
    {
        if (_mqttClients.TryGetValue(mqtt.Id, out var existingClient) && existingClient.IsConnected)
        {
            _logger.LogInformation($"{mqtt.ServerName}的Mqtt服务器连接已存在。");
            return;
        }

        _logger.LogInformation($"开始连接：{mqtt.ServerName}的服务器...");
        // mqtt.ConnectMessage = "开始连接服务器..."; // 移除此行，因为MqttServer没有这个属性

        var factory = new MqttFactory();
        var client = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
                      .WithClientId(mqtt.ClientId)
                      .WithTcpServer(mqtt.ServerUrl, mqtt.Port)
                      .WithCredentials(mqtt.Username, mqtt.Password)
                      .WithCleanSession()
                      .Build();

        client.UseConnectedHandler(async e => await HandleConnected(e, client, mqtt));
        client.UseApplicationMessageReceivedHandler(e => HandleMessageReceived(e, mqtt));
        client.UseDisconnectedHandler(async e => await HandleDisconnected(e, options, client, mqtt, stoppingToken));

        try
        {
            await client.ConnectAsync(options, stoppingToken);
            _mqttClients.AddOrUpdate(mqtt.Id, client, (id, oldClient) => client);
        }
        catch (Exception ex)
        {
            // mqtt.ConnectMessage = $"连接MQTT服务器失败: {ex.Message}"; // 移除此行，因为MqttServer没有这个属性
            _logger.LogError(ex, $"连接MQTT服务器失败: {mqtt.ServerName}");
        }
    }

    private static void HandleMessageReceived(MqttApplicationMessageReceivedEventArgs e, MqttServer mqtt)
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        // _logger.LogInformation($"MQTT客户端 {mqtt.ServerName} 收到消息: 主题={topic}, 消息={payload}");
    }

    private async Task HandleDisconnected(MqttClientDisconnectedEventArgs args, IMqttClientOptions options, IMqttClient client, MqttServer mqtt, CancellationToken stoppingToken)
    {
        _logger.LogWarning($"与MQTT服务器断开连接: {mqtt.ServerName}");
        // mqtt.ConnectMessage = "断开连接."; // 移除此行，因为MqttServer没有这个属性
        // mqtt.IsConnected = false; // 移除此行，因为MqttServer没有这个属性

        if (stoppingToken.IsCancellationRequested || !mqtt.IsActive) return;

        _reconnectAttempts.AddOrUpdate(mqtt.Id, 1, (id, count) => count + 1);
        var attempt = _reconnectAttempts[mqtt.Id];

        var delay = TimeSpan.FromSeconds(Math.Min(60, Math.Pow(2, attempt)));
        _logger.LogInformation($"与MQTT服务器：{mqtt.ServerName} 的连接已断开。将在 {delay.TotalSeconds} 秒后尝试第 {attempt} 次重新连接...");
        // mqtt.ConnectMessage = $"连接已断开，{delay.TotalSeconds}秒后尝试重连..."; // 移除此行，因为MqttServer没有这个属性

        await Task.Delay(delay, stoppingToken);

        try
        {
            // mqtt.ConnectMessage = "开始重新连接服务器..."; // 移除此行，因为MqttServer没有这个属性
            await client.ConnectAsync(options, stoppingToken);
        }
        catch (Exception ex)
        {
            // mqtt.ConnectMessage = "重新连接失败."; // 移除此行，因为MqttServer没有这个属性
            _logger.LogError(ex, $"重新与Mqtt服务器连接失败: {mqtt.ServerName}");
        }
    }

    private async Task HandleConnected(MqttClientConnectedEventArgs args, IMqttClient client, MqttServer mqtt)
    {
        _reconnectAttempts.TryRemove(mqtt.Id, out _);
        _logger.LogInformation($"已连接到MQTT服务器: {mqtt.ServerName}");
        // mqtt.IsConnected = true; // 移除此行，因为MqttServer没有这个属性
        // mqtt.ConnectMessage = "连接成功."; // 移除此行，因为MqttServer没有这个属性

        if (!string.IsNullOrEmpty(mqtt.SubscribeTopic))
        {
            await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(mqtt.SubscribeTopic).Build());
            _logger.LogInformation($"MQTT客户端 {mqtt.ServerName} 已订阅主题: {mqtt.SubscribeTopic}");
        }
    }

    private void HandleMqttListChanged(List<MqttServer> mqtts)
    {
        _logger.LogInformation("Mqtt列表发生了变化，正在重新加载数据...");
        _reloadSemaphore.Release();
    }
}