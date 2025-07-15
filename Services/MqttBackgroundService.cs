using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using PMSWPF.Helper;
using PMSWPF.Models;

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

        // 存储每个客户端重连尝试次数的字典
        private readonly Dictionary<int, int> _reconnectAttempts;


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
            _reconnectAttempts = new Dictionary<int, int>();
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

        private async void Execute()
        {
            while (!_stopEvent.WaitOne(0))
            {
                if (_dataServices.Mqtts == null || _dataServices.Mqtts.Count == 0)
                {
                    _reloadEvent.Reset();
                    continue;
                }

                _reloadEvent.WaitOne();

                try
                {
                    // 初始加载MQTT配置和变量数据。
                    var isLoaded = LoadMqttConfigurations();
                    if (isLoaded)
                    {
                        await ConnectMqttList();
                    }
                }
                catch (Exception e)
                {
                    NotificationHelper.ShowError($"Mqt后台服务主程序在加载变量和连接服务器过程中出现了错误：{e.Message} ", e);
                }
                finally
                {
                    _reloadEvent.Reset();
                }
            }
        }


        /// <summary>
        /// 停止MQTT后台服务。
        /// </summary>
        public async Task StopService()
        {
            NlogHelper.Info("Mqtt后台服务开始停止...."); // 记录服务停止信息

            _stopEvent.Set();
            // 取消订阅事件。
            _dataServices.OnMqttListChanged -= HandleMqttListChanged;

            await DisconnecetAll();

            // 清空所有字典。
            _mqttClients.Clear();
            _mqttConfigDic.Clear();
            _reconnectAttempts.Clear();
            NlogHelper.Info("Mqtt后台服务已停止。"); // 记录服务停止信息
        }

        private async Task DisconnecetAll()
        {
            // 断开所有已连接的MQTT客户端。
            foreach (var mqttId in _mqttClients.Keys.ToList())
            {
                try
                {
                    var client = _mqttClients[mqttId];
                    var mqtt = _mqttConfigDic[mqttId];
                    mqtt.IsConnected = false;
                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync();
                    }
                }
                catch (Exception e)
                {
                    NlogHelper.Error($"MqttID:{mqttId},断开连接的过程中发生了错误:{e.Message}",e);
                }
            }
        }


        /// <summary>
        /// 加载并连接MQTT配置。
        /// </summary>
        /// <returns>表示异步操作的任务。</returns>
        private bool LoadMqttConfigurations()
        {
            try
            {
                NlogHelper.Info("开始加载Mqtt配置文件...");
                _mqttConfigDic.Clear();
                // 从数据服务获取所有MQTT配置。
                var _mqttConfigList = _dataServices.Mqtts.Where(m => m.IsActive)
                                                   .ToList();
                foreach (var mqtt in _mqttConfigList)
                {
                    mqtt.OnMqttIsActiveChanged += OnMqttIsActiveChangedHandler;
                    _mqttConfigDic[mqtt.Id] = mqtt;
                    mqtt.ConnectMessage = "配置加载成功.";
                }

                NlogHelper.Info($"Mqtt配置文件加载成功，开启的Mqtt客户端：{_mqttConfigList.Count}个。");
                return true;
            }
            catch (Exception e)
            {
                NotificationHelper.ShowError($"Mqtt后台服务在加载变量的过程中发生了错误:{e.Message}");
                return false;
            }
        }

        private async void OnMqttIsActiveChangedHandler(Mqtt mqtt)
        {
            try
            {
                if (mqtt.IsActive)
                {
                    await ConnectMqtt(mqtt);
                }
                else
                {
                    if (!_mqttClients.TryGetValue(mqtt.Id, out var client))
                    {
                        NlogHelper.Warn($"没有在Mqtt连接字典中找到名字为：{mqtt.Name}的连接。");
                        return;
                    }

                    if (client.IsConnected)
                    {
                        await client.DisconnectAsync();
                    }

                    mqtt.IsConnected = false;
                    mqtt.ConnectMessage = "断开连接.";

                    _mqttClients.Remove(mqtt.Id);
                    NlogHelper.Info($"{mqtt.Name}的客户端，与服务器断开连接.");
                }
            }
            catch (Exception e)
            {
                NotificationHelper.ShowError($"{mqtt.Name}客户端，开启或关闭的过程中发生了错误:{e.Message}", e);
            }
        }

        /// <summary>
        /// 连接到指定的MQTT代理。
        /// </summary>
        /// <param name="mqtt">MQTT配置对象。</param>
        /// <returns>表示异步操作的任务。</returns>
        private async Task ConnectMqttList()
        {
            foreach (Mqtt mqtt in _mqttConfigDic.Values.ToList())
            {
                try
                {
                    if (_mqttClients.TryGetValue(mqtt.Id, out var mclient) && mclient.IsConnected)
                    {
                        NlogHelper.Info($"{mqtt.Name}的Mqtt服务器连接已存在。");
                        mqtt.ConnectMessage = "连接成功.";
                        mqtt.IsConnected = true;
                        continue;
                    }

                    await ConnectMqtt(mqtt);
                }
                catch (Exception ex)
                {
                    NotificationHelper.ShowError($"连接MQTT服务器失败: {mqtt.Name} - {ex.Message}", ex);
                }
            }
        }

        private async Task ConnectMqtt(Mqtt mqtt)
        {
            NlogHelper.Info($"开始连接：{mqtt.Name}的服务器...");
            mqtt.ConnectMessage = "开始连接服务器...";
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
            client.UseConnectedHandler(async (e) => { await HandleConnected(e, client, mqtt); });

            // 设置接收消息处理程序
            client.UseApplicationMessageReceivedHandler(e => { HandleMessageReceived(e, mqtt); });

            // 设置断开连接事件处理程序。
            client.UseDisconnectedHandler(async (e) => await HandleDisconnected(e, options, client, mqtt));

            // 尝试连接到MQTT代理。
            await client.ConnectAsync(options, CancellationToken.None);

            // 将连接成功的客户端添加到字典。
            _mqttClients[mqtt.Id] = client;
        }

        private static void HandleMessageReceived(MqttApplicationMessageReceivedEventArgs e, Mqtt mqtt)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            NlogHelper.Info($"MQTT客户端 {mqtt.Name} 收到消息: 主题={topic}, 消息={payload}");

            // 在这里添加处理消息的逻辑
        }

        private async Task HandleDisconnected(MqttClientDisconnectedEventArgs args, IMqttClientOptions options,
                                              IMqttClient client,
                                              Mqtt mqtt)
        {
            NotificationHelper.ShowWarn($"与MQTT服务器断开连接: {mqtt.Name}");
            mqtt.ConnectMessage = "断开连接.";
            mqtt.IsConnected = false;
            // 服务停止
            if (_stopEvent.WaitOne(0) || !mqtt.IsActive)
                return;

            // 增加重连尝试次数
            if (!_reconnectAttempts.ContainsKey(mqtt.Id))
            {
                _reconnectAttempts[mqtt.Id] = 0;
            }

            _reconnectAttempts[mqtt.Id]++;

            // 指数退避策略
            var maxDelay = TimeSpan.FromMinutes(5); // 最大延迟5分钟
            var baseDelay = TimeSpan.FromSeconds(5); // 基础延迟5秒
            var delay = TimeSpan.FromSeconds(baseDelay.TotalSeconds *
                                             Math.Pow(2, _reconnectAttempts[mqtt.Id] - 1));
            if (delay > maxDelay)
            {
                delay = maxDelay;
            }

            NlogHelper.Info(
                $"与MQTT服务器：{mqtt.Name} 的连接已断开。将在 {delay.TotalSeconds} 秒后尝试第 {_reconnectAttempts[mqtt.Id]} 次重新连接...");

            mqtt.ConnectMessage = $"连接已断开。将在 {delay.TotalSeconds} 秒后尝试第 {_reconnectAttempts[mqtt.Id]} 次重新连接...";
            await Task.Delay(delay);
            try
            {
                mqtt.ConnectMessage = $"开始重新连接服务器...";
                await client.ConnectAsync(options, CancellationToken.None);
            }
            catch (Exception ex)
            {
                mqtt.ConnectMessage = $"重新与Mqtt服务器连接失败.";
                NlogHelper.Error($"重新与Mqtt服务器连接失败: {mqtt.Name}", ex);
            }
        }

        private async Task HandleConnected(MqttClientConnectedEventArgs args, IMqttClient client, Mqtt mqtt)
        {
            // 重置重连尝试次数
            if (_reconnectAttempts.ContainsKey(mqtt.Id))
            {
                _reconnectAttempts[mqtt.Id] = 0;
            }

            NotificationHelper.ShowSuccess($"已连接到MQTT服务器: {mqtt.Name}");
            mqtt.IsConnected = true;
            mqtt.ConnectMessage = "连接成功.";

            // 订阅主题
            await client.SubscribeAsync(new MqttTopicFilterBuilder().WithTopic(mqtt.SubTopic)
                                                                    .Build());
            NlogHelper.Info($"MQTT客户端 {mqtt.Name} 已订阅主题: {mqtt.SubTopic}");
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