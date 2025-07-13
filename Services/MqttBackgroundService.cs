using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
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
    public class MqttBackgroundService
    {
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
        /// 启动MQTT后台服务。
        /// </summary>
        public async void StartService()
        {
            NlogHelper.Info("MqttBackgroundService started."); // 记录服务启动信息

            // 订阅MQTT列表和变量数据变化的事件，以便在数据更新时重新加载配置和数据。
            _dataServices.OnMqttListChanged += HandleMqttListChanged;
            _dataServices.OnDeviceListChanged += HandleDeviceListChanged;

            // 初始加载MQTT配置和变量数据。
            await LoadMqttConfigurations();
            await LoadVariableData();

            // 初始化定时器，每5秒执行一次DoWork方法，用于周期性地发布数据。
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5)); // 每5秒轮询一次

            // 使服务保持运行，直到收到停止请求。
            // await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async void HandleDeviceListChanged( List<Device> devices)
        {
            NlogHelper.Info("Variable data changed. Reloading variable associations."); // 记录变量数据变化信息
            // 重新加载变量数据。
            await LoadVariableData();
        }

        /// <summary>
        /// 停止MQTT后台服务。
        /// </summary>
        public async void StopService()
        {
            NlogHelper.Info("MqttBackgroundService stopping."); // 记录服务停止信息

            // 停止定时器。
            _timer?.Change(Timeout.Infinite, 0);

            // 取消订阅事件。
            _dataServices.OnMqttListChanged -= HandleMqttListChanged;
            _dataServices.OnDeviceListChanged -= HandleDeviceListChanged;

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
                                NlogHelper.Info(
                                    $"Published {variable.Name} = {variable.DataValue} to {topic}/{variable.Name}",
                                    throttle: true); // 记录发布信息
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
            var allMqtts = await _dataServices.GetMqttsAsync();
            var activeMqtts = allMqtts.Where(m => m.IsActive)
                                      .ToList();
            var activeMqttIds = activeMqtts.Select(m => m.Id)
                                           .ToHashSet();

            // 断开并移除不再活跃或已删除的MQTT客户端。
            var clientsToDisconnect = _mqttClients.Keys.Except(activeMqttIds)
                                                  .ToList();
            foreach (var id in clientsToDisconnect)
            {
                if (_mqttClients.TryGetValue(id, out var client))
                {
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync();
                        // 更新模型中的连接状态
                        if (_mqttConfigurations.TryGetValue(id, out var mqttConfig))
                        {
                            mqttConfig.IsConnected = false;
                        }
                    }

                    _mqttClients.Remove(id);
                    NlogHelper.Info(
                        $"Disconnected and removed MQTT client for ID: {id} (no longer active or removed).");
                }

                _mqttConfigurations.Remove(id);
                _mqttVariableData.Remove(id);
            }

            // 连接或更新活跃的客户端。
            foreach (var mqtt in activeMqtts)
            {
                if (!_mqttClients.ContainsKey(mqtt.Id))
                {
                    await ConnectMqttClient(mqtt);
                }

                // 始终更新或添加MQTT配置到字典。
                _mqttConfigurations[mqtt.Id] = mqtt;
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
                    NlogHelper.Info($"Connected to MQTT broker: {mqtt.Name}");
                    NotificationHelper.ShowSuccess($"已连接到MQTT服务器: {mqtt.Name}");
                    mqtt.IsConnected = true;
                });

                // 设置断开连接事件处理程序。
                client.UseDisconnectedHandler(async e =>
                {
                    NlogHelper.Warn($"Disconnected from MQTT broker: {mqtt.Name}. Reason: {e.Reason}");
                    NotificationHelper.ShowInfo($"与MQTT服务器断开连接: {mqtt.Name}");
                    mqtt.IsConnected = false;
                    // 尝试重新连接。
                    await Task.Delay(TimeSpan.FromSeconds(5)); // 等待5秒后重连
                    try
                    {
                        await client.ConnectAsync(options, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        NlogHelper.Error($"Failed to reconnect to MQTT broker: {mqtt.Name}", ex);
                    }
                });

                // 尝试连接到MQTT代理。
                await client.ConnectAsync(options, CancellationToken.None);
                // 将连接成功的客户端添加到字典。
                _mqttClients[mqtt.Id] = client;
            }
            catch (Exception ex)
            {
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
            var allVariables = _dataServices.VariableDatas;
            if (!allVariables.Any())
                return;
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
                        _mqttVariableData[mqtt.Id]
                            .Add(variable);
                    }
                }
            }
        }

        /// <summary>
        /// 处理MQTT列表变化事件的回调方法。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="mqtts">更新后的MQTT配置列表。</param>
        private async void HandleMqttListChanged( List<Mqtt> mqtts)
        {
            NlogHelper.Info("MQTT list changed. Reloading configurations."); // 记录MQTT列表变化信息
            // 重新加载MQTT配置和变量数据。
            await LoadMqttConfigurations();
            await LoadVariableData(); // 重新加载变量数据，以防关联发生变化
        }

    }
}