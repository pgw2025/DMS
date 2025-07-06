using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using PMSWPF.Models;
using PMSWPF.Services;
using PMSWPF.Helper;
using PMSWPF.Enums;

namespace PMSWPF.Services
{
    /// <summary>
    /// MQTT后台服务，继承自BackgroundService，用于在后台管理MQTT连接和数据发布。
    /// </summary>
    public class MqttBackgroundService : BackgroundService
    {
        // NLog日志记录器，用于记录服务运行时的信息、警告和错误。
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        // 数据服务实例，用于访问和操作应用程序数据，如MQTT配置和变量数据。
        private readonly DataServices _dataServices;
        // 存储MQTT客户端实例的字典，键为MQTT配置ID，值为IMqttClient对象。
        private readonly Dictionary<int, IMqttClient> _mqttClients;
        // 存储MQTT配置的字典，键为MQTT配置ID，值为Mqtt模型对象。
        private readonly Dictionary<int, Mqtt> _mqttConfigurations;
        // 存储与MQTT配置关联的变量数据的字典，键为MQTT配置ID，值为VariableData列表。
        private readonly Dictionary<int, List<VariableData>> _mqttVariableData;
        // 定时器，用于周期性地执行数据发布任务。
        private Timer _timer;

        /// <summary>
        /// 构造函数，注入DataServices。
        /// </summary>
        /// <param name="dataServices">数据服务实例。</param>
        public MqttBackgroundService(DataServices dataServices)
        {
            _dataServices = dataServices;
            _mqttClients = new Dictionary<int, IMqttClient>();
            _mqttConfigurations = new Dictionary<int, Mqtt>();
            _mqttVariableData = new Dictionary<int, List<VariableData>>();
        }

        /// <summary>
        /// 后台服务的执行方法，当服务启动时调用。
        /// </summary>
        /// <param name="stoppingToken">用于取消操作的CancellationToken。</param>
        /// <returns>表示异步操作的任务。</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Logger.Info("MqttBackgroundService started."); // 记录服务启动信息

            // 订阅MQTT列表和变量数据变化的事件，以便在数据更新时重新加载配置和数据。
            _dataServices.OnMqttListChanged += HandleMqttListChanged;
            _dataServices.OnVariableDataChanged += HandleVariableDataChanged;

            // 初始加载MQTT配置和变量数据。
            await LoadMqttConfigurations();
            await LoadVariableData();

            // 初始化定时器，每5秒执行一次DoWork方法，用于周期性地发布数据。
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // 每5秒轮询一次

            // 使服务保持运行，直到收到停止请求。
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        /// <summary>
        /// 后台服务的停止方法，当服务停止时调用。
        /// </summary>
        /// <param name="stoppingToken">用于取消操作的CancellationToken。</param>
        /// <returns>表示异步操作的任务。</returns>
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            Logger.Info("MqttBackgroundService stopping."); // 记录服务停止信息

            // 停止定时器。
            _timer?.Change(Timeout.Infinite, 0);

            // 取消订阅事件。
            _dataServices.OnMqttListChanged -= HandleMqttListChanged;
            _dataServices.OnVariableDataChanged -= HandleVariableDataChanged;

            // 断开所有已连接的MQTT客户端。
            foreach (var client in _mqttClients.Values)
            {
                if (client.IsConnected)
                {
                    await client.DisconnectAsync();
                }
            }
            // 清空所有字典。
            _mqttClients.Clear();
            _mqttConfigurations.Clear();
            _mqttVariableData.Clear();

            await base.StopAsync(stoppingToken);
        }

        /// <summary>
        /// 定时器回调方法，用于周期性地检查并发布已修改的变量数据。
        /// </summary>
        /// <param name="state">定时器状态对象（此处未使用）。</param>
        private async void DoWork(object state)
        {
            // 遍历所有MQTT配置关联的变量数据。
            foreach (var mqttConfigId in _mqttVariableData.Keys)
            {
                // 检查MQTT客户端是否连接。
                if (_mqttClients.TryGetValue(mqttConfigId, out var client) && client.IsConnected)
                {
                    var variables = _mqttVariableData[mqttConfigId];
                    // 遍历与当前MQTT配置关联的变量。
                    foreach (var variable in variables)
                    {
                        // 如果变量已被修改（IsModified标志为true）。
                        if (variable.IsModified) 
                        {
                            // 获取发布主题。
                            var topic = _mqttConfigurations[mqttConfigId].PublishTopic;
                            if (!string.IsNullOrEmpty(topic))
                            {
                                // 构建MQTT消息。
                                var message = new MqttApplicationMessageBuilder()
                                    .WithTopic($"{topic}/{variable.Name}") // 主题格式：PublishTopic/VariableName
                                    .WithPayload(variable.DataValue) // 消息载荷为变量的值
                                    .Build();

                                // 发布MQTT消息。
                                await client.PublishAsync(message);
                                Logger.Info($"Published {variable.Name} = {variable.DataValue} to {topic}/{variable.Name}"); // 记录发布信息
                                variable.IsModified = false; // 发布后重置修改标志。
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 加载并连接MQTT配置。
        /// </summary>
        /// <returns>表示异步操作的任务。</returns>
        private async Task LoadMqttConfigurations()
        {
            // 从数据服务获取所有MQTT配置。
            var mqtts = await _dataServices.GetMqttsAsync();
            foreach (var mqtt in mqtts)
            {
                // 如果客户端字典中不包含当前MQTT配置的客户端，则尝试连接。
                if (!_mqttClients.ContainsKey(mqtt.Id))
                {
                    await ConnectMqttClient(mqtt);
                }
                // 更新或添加MQTT配置到字典。
                _mqttConfigurations[mqtt.Id] = mqtt;
            }

            // 断开并移除不再配置中的MQTT客户端。
            var removedMqttIds = _mqttClients.Keys.Except(mqtts.Select(m => m.Id)).ToList();
            foreach (var id in removedMqttIds)
            {
                if (_mqttClients.ContainsKey(id))
                {
                    var client = _mqttClients[id];
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync();
                    }
                    _mqttClients.Remove(id);
                    Logger.Info($"Disconnected and removed MQTT client for ID: {id}");
                }
                _mqttConfigurations.Remove(id);
                _mqttVariableData.Remove(id);
            }
        }

        /// <summary>
        /// 连接到指定的MQTT代理。
        /// </summary>
        /// <param name="mqtt">MQTT配置对象。</param>
        /// <returns>表示异步操作的任务。</returns>
        private async Task ConnectMqttClient(Mqtt mqtt)
        {
            try
            {
                // 创建MQTT客户端工厂和客户端实例。
                var factory = new MqttFactory();
                var client = factory.CreateMqttClient();
                // 构建MQTT客户端连接选项。
                var options = new MqttClientOptionsBuilder()
                    .WithClientId(mqtt.ClientID)
                    .WithTcpServer(mqtt.Host, mqtt.Port)
                    .WithCredentials(mqtt.UserName, mqtt.PassWord)
                    .WithCleanSession() // 清理会话，每次连接都是新会话
                    .Build();

                // 设置连接成功事件处理程序。
                client.UseConnectedHandler(e =>
                {
                    Logger.Info($"Connected to MQTT broker: {mqtt.Name}");
                    NotificationHelper.ShowSuccess($"已连接到MQTT服务器: {mqtt.Name}");
                });

                // 设置断开连接事件处理程序。
                client.UseDisconnectedHandler(async e =>
                {
                    Logger.Warn($"Disconnected from MQTT broker: {mqtt.Name}. Reason: {e.Reason}");
                    NotificationHelper.ShowInfo($"与MQTT服务器断开连接: {mqtt.Name}");
                    // 尝试重新连接。
                    await Task.Delay(TimeSpan.FromSeconds(5)); // 等待5秒后重连
                    try
                    {
                        await client.ConnectAsync(options, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, $"Failed to reconnect to MQTT broker: {mqtt.Name}");
                    }
                });

                // 尝试连接到MQTT代理。
                await client.ConnectAsync(options, CancellationToken.None);
                // 将连接成功的客户端添加到字典。
                _mqttClients[mqtt.Id] = client;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to connect to MQTT broker: {mqtt.Name}");
                NotificationHelper.ShowError($"连接MQTT服务器失败: {mqtt.Name} - {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 加载所有变量数据并按MQTT配置ID进行分组。
        /// </summary>
        /// <returns>表示异步操作的任务。</returns>
        private async Task LoadVariableData()
        {
            // 从数据服务获取所有变量数据。
            var allVariables = await _dataServices.GetAllVariableDataAsync();
            _mqttVariableData.Clear(); // 清空现有数据

            // 遍历所有变量，并根据其关联的MQTT配置进行分组。
            foreach (var variable in allVariables)
            {
                if (variable.Mqtts != null)
                {
                    foreach (var mqtt in variable.Mqtts)
                    {
                        // 如果字典中没有该MQTT配置的条目，则创建一个新的列表。
                        if (!_mqttVariableData.ContainsKey(mqtt.Id))
                        {
                            _mqttVariableData[mqtt.Id] = new List<VariableData>();
                        }
                        // 将变量添加到对应MQTT配置的列表中。
                        _mqttVariableData[mqtt.Id].Add(variable);
                    }
                }
            }
        }

        /// <summary>
        /// 处理MQTT列表变化事件的回调方法。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="mqtts">更新后的MQTT配置列表。</param>
        private async void HandleMqttListChanged(object sender, List<Mqtt> mqtts)
        {
            Logger.Info("MQTT list changed. Reloading configurations."); // 记录MQTT列表变化信息
            // 重新加载MQTT配置和变量数据。
            await LoadMqttConfigurations();
            await LoadVariableData(); // 重新加载变量数据，以防关联发生变化
        }

        /// <summary>
        /// 处理变量数据变化事件的回调方法。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="variableDatas">更新后的变量数据列表。</param>
        private async void HandleVariableDataChanged(object sender, List<VariableData> variableDatas)
        {
            Logger.Info("Variable data changed. Reloading variable associations."); // 记录变量数据变化信息
            // 重新加载变量数据。
            await LoadVariableData();
        }
    }
}
