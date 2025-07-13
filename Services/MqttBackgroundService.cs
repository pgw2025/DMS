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
        private readonly Dictionary<int, Mqtt> _mqttConfigDic;


        // 定时器，用于周期性地执行数据发布任务。
        private Timer _timer;
        private Thread _serviceMainThread;

        private ManualResetEvent _reloadEvent = new ManualResetEvent(false);
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);

        /// <summary>
        /// 构造函数，注入DataServices。
        /// </summary>
        /// <param name="dataServices">数据服务实例。</param>
        public MqttBackgroundService(DataServices dataServices)
        {
            _dataServices = dataServices;
            _mqttClients = new Dictionary<int, IMqttClient>();
            _mqttConfigDic = new Dictionary<int, Mqtt>();
        }

        /// <summary>
        /// 启动MQTT后台服务。
        /// </summary>
        public async void StartService()
        {
            // 订阅MQTT列表和变量数据变化的事件，以便在数据更新时重新加载配置和数据。
            _dataServices.OnMqttListChanged += HandleMqttListChanged;
            NlogHelper.Info("Mqtt后台服务启动"); // 记录服务启动信息
            _reloadEvent.Set();
            _stopEvent.Reset();
            _serviceMainThread = new Thread(Execute);
            _serviceMainThread.IsBackground = true;
            _serviceMainThread.Name = "MqttServiceMainThread";
            _serviceMainThread.Start();
            
        }

        private void Execute()
        {
            while (!_stopEvent.WaitOne(0))
            {
                if (_dataServices.Mqtts==null || _dataServices.Mqtts.Count==0)
                {
                    _reloadEvent.Reset();
                    continue;
                }
                
                _reloadEvent.WaitOne();
                // 初始加载MQTT配置和变量数据。
                LoadMqttConfigurations();
                ConnectMqttClient();
                
                _reloadEvent.Reset();
            }
        }


        /// <summary>
        /// 停止MQTT后台服务。
        /// </summary>
        public  void StopService()
        {
            NlogHelper.Info("Mqtt后台服务开始停止...."); // 记录服务停止信息

            _stopEvent.Set();
            // 取消订阅事件。
            _dataServices.OnMqttListChanged -= HandleMqttListChanged;

            // 断开所有已连接的MQTT客户端。
            foreach (var mqttId in _mqttClients.Keys.ToList())
            {
                var client=_mqttClients[mqttId];
                var mqtt=_mqttConfigDic[mqttId];
                mqtt.IsConnected = false;
                if (client.IsConnected)
                {
                    client.DisconnectAsync().GetAwaiter().GetResult();
                }
            }

            // 清空所有字典。
            _mqttClients.Clear();
            _mqttConfigDic.Clear();
            NlogHelper.Info("Mqtt后台服务已停止。"); // 记录服务停止信息
        }


        /// <summary>
        /// 加载并连接MQTT配置。
        /// </summary>
        /// <returns>表示异步操作的任务。</returns>
        private void LoadMqttConfigurations()
        {
            NlogHelper.Info("开始加载Mqtt配置文件...");
            // 从数据服务获取所有MQTT配置。
            var _mqttConfigList = _dataServices.Mqtts.Where(m => m.IsActive)
                                               .ToList();
            foreach (var mqtt in _mqttConfigList)
            {
                _mqttConfigDic[mqtt.Id] = mqtt;
            }
            NlogHelper.Info($"Mqtt配置文件加载成功，开启的Mqtt客户端：{_mqttConfigList.Count}个。");
        }

        /// <summary>
        /// 连接到指定的MQTT代理。
        /// </summary>
        /// <param name="mqtt">MQTT配置对象。</param>
        /// <returns>表示异步操作的任务。</returns>
        private void ConnectMqttClient()
        {
            foreach (Mqtt mqtt in _mqttConfigDic.Values.ToList())
            {
                try
                {
                    NlogHelper.Info($"开始连接：{mqtt.Name}的服务器...");
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
                        NotificationHelper.ShowSuccess($"已连接到MQTT服务器: {mqtt.Name}");
                        mqtt.IsConnected = true;
                    });

                    // 设置断开连接事件处理程序。
                    client.UseDisconnectedHandler(async e =>
                    {
                        NotificationHelper.ShowWarn($"与MQTT服务器断开连接: {mqtt.Name}");
                        mqtt.IsConnected = false;
                        // 服务停止
                        if (_stopEvent.WaitOne(0))
                            return;
                        
                        NlogHelper.Info($"5秒后重新连接Mqtt服务器：{mqtt.Name}");
                        // 尝试重新连接。
                        await Task.Delay(TimeSpan.FromSeconds(5)); // 等待5秒后重连
                        try
                        {
                            await client.ConnectAsync(options, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            NlogHelper.Error($"重新与Mqtt服务器连接失败: {mqtt.Name}", ex);
                        }
                    });

                    // 尝试连接到MQTT代理。
                    client.ConnectAsync(options, CancellationToken.None)
                          .GetAwaiter()
                          .GetResult();
                    // 将连接成功的客户端添加到字典。
                    _mqttClients[mqtt.Id] = client;
                }
                catch (Exception ex)
                {
                    NotificationHelper.ShowError($"连接MQTT服务器失败: {mqtt.Name} - {ex.Message}", ex);
                }
            }
        }


        /// <summary>
        /// 处理MQTT列表变化事件的回调方法。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="mqtts">更新后的MQTT配置列表。</param>
        private async void HandleMqttListChanged(List<Mqtt> mqtts)
        {
            NlogHelper.Info("Mqtt列表发生了变化，正在重新加载数据..."); // 记录MQTT列表变化信息
            // 重新加载MQTT配置和变量数据。
            _reloadEvent.Set();
        }
    }
}