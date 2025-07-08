using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Models;

namespace PMSWPF.Services
{
    /// <summary>
    /// OPC UA 后台服务，负责处理所有与 OPC UA 相关的操作，包括连接服务器、订阅变量和接收数据更新。
    /// </summary>
    /// <remarks>
    /// ## 调用逻辑
    /// 1.  **实例化与启动**:
    ///     - 在 `App.xaml.cs` 的 `ConfigureServices` 方法中，通过 `services.AddSingleton<OpcUaBackgroundService>()` 将该服务注册为单例。
    ///     - 在 `OnStartup` 方法中，通过 `Host.Services.GetRequiredService<OpcUaBackgroundService>().StartAsync()` 启动服务。
    ///
    /// 2.  **核心执行流程 (`ExecuteAsync`)**:
    ///     - 服务启动后，`ExecuteAsync` 方法被调用。
    ///     - 订阅 `DataServices.OnVariableDataChanged` 事件，以便在变量配置发生变化时动态更新 OPC UA 的连接和订阅。
    ///     - 调用 `LoadOpcUaVariables()` 方法，初始化加载所有协议类型为 `OpcUA` 且状态为激活的变量。
    ///     - 进入主循环，等待应用程序关闭信号。
    ///
    /// 3.  **变量加载与管理 (`LoadOpcUaVariables`)**:
    ///     - 从 `DataServices` 获取所有变量数据。
    ///     - 清理本地缓存中已不存在或不再活跃的变量，并断开相应的 OPC UA 连接。
    ///     - 遍历所有活动的 OPC UA 变量，为新增的变量调用 `ConnectAndSubscribeOpcUa` 方法建立连接和订阅。
    ///     - 如果变量已存在但会话断开，则尝试重新连接。
    ///
    /// 4.  **连接与订阅 (`ConnectAndSubscribeOpcUa`)**:
    ///     - 检查变量是否包含有效的终结点 URL 和节点 ID。
    ///     - 如果到目标终结点的会话已存在且已连接，则复用该会话。
    ///     - 否则，创建一个新的 OPC UA `Session`，并将其缓存到 `_opcUaSessions` 字典中。
    ///     - 创建一个 `Subscription`，并将其缓存到 `_opcUaSubscriptions` 字典中。
    ///     - 创建一个 `MonitoredItem` 来监控指定变量节点的值变化，并将其添加到订阅中。
    ///     - 注册 `OnNotification` 事件回调，用于处理接收到的数据更新。
    ///
    /// 5.  **数据接收 (`OnNotification`)**:
    ///     - 当订阅的变量值发生变化时，OPC UA 服务器会发送通知。
    ///     - `OnNotification` 方法被触发，处理接收到的数据。
    ///
    /// 6.  **动态更新 (`HandleVariableDataChanged`)**:
    ///     - 当 `DataServices` 中的变量数据发生增、删、改时，会触发 `OnVariableDataChanged` 事件。
    ///     - `HandleVariableDataChanged` 方法被调用，重新执行 `LoadOpcUaVariables()` 以应用最新的变量配置。
    ///
    /// 7.  **服务停止 (`ExecuteAsync` 退出循环)**:
    ///     - 当应用程序关闭时，`stoppingToken` 被触发。
    ///     - 取消对 `OnVariableDataChanged` 事件的订阅。
    ///     - 调用 `DisconnectAllOpcUaSessions()` 方法，关闭所有活动的 OPC UA 会话和订阅。
    /// </remarks>
    public class OpcUaBackgroundService
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _executingTask;
        private readonly DataServices _dataServices;
        // 存储 OPC UA 会话，键为终结点 URL，值为会话对象。
        private readonly Dictionary<string, Session> _opcUaSessions;
        // 存储 OPC UA 订阅，键为终结点 URL，值为订阅对象。
        private readonly Dictionary<string, Subscription> _opcUaSubscriptions;
        // 存储活动的 OPC UA 变量，键为变量的唯一 ID。
        private readonly Dictionary<int, VariableData> _opcUaVariables; // Key: VariableData.Id

        /// <summary>
        /// OpcUaBackgroundService 的构造函数。
        /// </summary>
        /// <param name="dataServices">数据服务，用于访问数据库中的变量信息。</param>
        public OpcUaBackgroundService(DataServices dataServices)
        {
            _dataServices = dataServices;
            _opcUaSessions = new Dictionary<string, Session>();
            _opcUaSubscriptions = new Dictionary<string, Subscription>();
            _opcUaVariables = new Dictionary<int, VariableData>();
        }

        /// <summary>
        /// 后台服务的主执行方法。
        /// </summary>
        /// <param name="stoppingToken">用于通知服务停止的取消令牌。</param>
        /// <returns>表示异步操作的任务。</returns>
        public void StartService()
        {
            NlogHelper.Info("OPC UA 服务正在启动...");
            _cancellationTokenSource = new CancellationTokenSource();
            _executingTask = ExecuteAsync(_cancellationTokenSource.Token);
        }

        public async Task StopService()
        {
            NlogHelper.Info("OPC UA 服务正在停止...");
            if (_executingTask == null)
            {
                return;
            }

            try
            {
                _cancellationTokenSource.Cancel();
            }
            finally
            {
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, CancellationToken.None));
            }
        }

        protected async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            NlogHelper.Info("OpcUaBackgroundService started.");

            // 订阅变量数据变化事件，以便在变量配置发生变化时重新加载。
            _dataServices.OnVariableDataChanged += HandleVariableDataChanged;

            // 初始化时加载所有活动的 OPC UA 变量。
            await LoadOpcUaVariables();

            // 循环运行，直到接收到停止信号。
            while (!stoppingToken.IsCancellationRequested)
            {
                // 可以在这里添加周期性任务，例如检查和重新连接断开的会话。
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            NlogHelper.Info("OpcUaBackgroundService stopping.");
            // 服务停止时，取消订阅事件并断开所有 OPC UA 连接。
            _dataServices.OnVariableDataChanged -= HandleVariableDataChanged;
            await DisconnectAllOpcUaSessions();
        }

        /// <summary>
        /// 从数据库加载所有活动的 OPC UA 变量，并进行相应的连接和订阅管理。
        /// </summary>
        private async Task LoadOpcUaVariables()
        {
            NlogHelper.Info("正在加载 OPC UA 变量...");
            var allVariables = await _dataServices.GetAllVariableDataAsync();
            var opcUaVariables = allVariables.Where(v => v.ProtocolType == ProtocolType.OpcUA && v.IsActive).ToList();

            // 清理不再活跃或已删除的变量。
            var currentOpcUaVariableIds = opcUaVariables.Select(v => v.Id).ToHashSet();
            var variablesToRemove = _opcUaVariables.Keys.Except(currentOpcUaVariableIds).ToList();

            NlogHelper.Info($"发现 {variablesToRemove.Count} 个要移除的 OPC UA 变量。");

            foreach (var id in variablesToRemove)
            {
                if (_opcUaVariables.TryGetValue(id, out var variable))
                {
                    // 断开与该变量相关的 OPC UA 会话。
                    await DisconnectOpcUaSession(variable.OpcUaEndpointUrl);
                    _opcUaVariables.Remove(id);
                }
            }

            // 处理新增或更新的变量。
            foreach (var variable in opcUaVariables)
            {
                if (!_opcUaVariables.ContainsKey(variable.Id))
                {
                    // 如果是新变量，则添加到字典并建立连接和订阅。
                    _opcUaVariables.Add(variable.Id, variable);
                    await ConnectAndSubscribeOpcUa(variable);
                }
                else
                {
                    // 如果变量已存在，则更新其信息。
                    _opcUaVariables[variable.Id] = variable;
                    // 如果终结点 URL 对应的会话已断开，则尝试重新连接。
                    if (_opcUaSessions.ContainsKey(variable.OpcUaEndpointUrl) && !_opcUaSessions[variable.OpcUaEndpointUrl].Connected)
                    {
                        await ConnectAndSubscribeOpcUa(variable);
                    }
                }
            }
        }

        /// <summary>
        /// 连接到 OPC UA 服务器并订阅指定的变量。
        /// </summary>
        /// <param name="variable">要订阅的变量信息。</param>
        private async Task ConnectAndSubscribeOpcUa(VariableData variable)
        {
            NlogHelper.Info($"正在为变量 '{variable.Name}' 连接和订阅 OPC UA 服务器...");
            if (string.IsNullOrEmpty(variable.OpcUaEndpointUrl) || string.IsNullOrEmpty(variable.OpcUaNodeId))
            {
                NlogHelper.Warn($"OPC UA variable {variable.Name} has invalid EndpointUrl or NodeId.");
                return;
            }

            Session session = null;
            // 检查是否已存在到该终结点的活动会话。
            if (_opcUaSessions.TryGetValue(variable.OpcUaEndpointUrl, out session) && session.Connected)
            {
                NlogHelper.Info($"Already connected to OPC UA endpoint: {variable.OpcUaEndpointUrl}");
            }
            else
            {
                try
                {
                    // 1. 创建应用程序实例。
                    ApplicationInstance application = new ApplicationInstance
                    {
                        ApplicationName = "PMSWPF OPC UA Client",
                        ApplicationType = ApplicationType.Client,
                        ConfigSectionName = "PMSWPF.OpcUaClient"
                    };

                    // 2. 加载应用程序配置。
                    ApplicationConfiguration config = await application.LoadApplicationConfiguration(false);

                    // 3. 检查应用程序实例证书。
                    bool haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);
                    if (!haveAppCertificate)
                    {
                        throw new Exception("Application instance certificate invalid!");
                    }

                    // 4. 发现服务器提供的终结点。
                    DiscoveryClient discoveryClient = DiscoveryClient.Create(new Uri(variable.OpcUaEndpointUrl));
                    EndpointDescriptionCollection endpoints = discoveryClient.GetEndpoints(new Opc.Ua.StringCollection { variable.OpcUaEndpointUrl });
                    
                    // 简化处理：选择第一个无安全策略的终结点。在生产环境中应选择合适的安全策略。
                    // ConfiguredEndpoint configuredEndpoint = new ConfiguredEndpoint(null, endpoints.First(e => e.SecurityMode == MessageSecurityMode.None), config);
                    EndpointDescription selectedEndpoint = CoreClientUtils.SelectEndpoint(application.ApplicationConfiguration, variable.OpcUaEndpointUrl, false);
                    EndpointConfiguration endpointConfiguration = EndpointConfiguration.Create(application.ApplicationConfiguration);
                    ConfiguredEndpoint configuredEndpoint = new ConfiguredEndpoint(null, selectedEndpoint, endpointConfiguration);

                    // 5. 创建会话。
                    session = await Session.Create(
                        config,
                        configuredEndpoint,
                        false, // 不更新证书
                        "PMSWPF OPC UA Session", // 会话名称
                        60000, // 会话超时时间（毫秒）
                        new UserIdentity(new AnonymousIdentityToken()), // 使用匿名用户身份
                        null);

                    _opcUaSessions[variable.OpcUaEndpointUrl] = session;
                    NlogHelper.Info($"Connected to OPC UA server: {variable.OpcUaEndpointUrl}");
                    NotificationHelper.ShowSuccess($"已连接到 OPC UA 服务器: {variable.OpcUaEndpointUrl}");

                    // 6. 创建订阅。
                    Subscription subscription = new Subscription(session.DefaultSubscription);
                    subscription.PublishingInterval = 1000; // 发布间隔（毫秒）
                    session.AddSubscription(subscription);
                    subscription.Create();

                    _opcUaSubscriptions[variable.OpcUaEndpointUrl] = subscription;

                    // 7. 创建监控项并添加到订阅中。
                    MonitoredItem monitoredItem = new MonitoredItem(subscription.DefaultItem);
                    monitoredItem.DisplayName = variable.Name;
                    monitoredItem.StartNodeId = new NodeId(variable.OpcUaNodeId); // 设置要监控的节点 ID
                    monitoredItem.AttributeId = Attributes.Value; // 监控节点的值属性
                    monitoredItem.SamplingInterval = 1000; // 采样间隔（毫秒）
                    monitoredItem.QueueSize = 1; // 队列大小
                    monitoredItem.DiscardOldest = true; // 丢弃最旧的数据
                    // 注册数据变化通知事件。
                    monitoredItem.Notification += OnNotification;
                        
                    subscription.AddItem(monitoredItem);
                    subscription.ApplyChanges(); // 应用更改
                }
                catch (Exception ex)
                {
                    NlogHelper.Error($"连接或订阅 OPC UA 服务器失败: {variable.OpcUaEndpointUrl} - {ex.Message}", ex);
                    NotificationHelper.ShowError($"连接或订阅 OPC UA 服务器失败: {variable.OpcUaEndpointUrl} - {ex.Message}", ex);
                }
            }
        }

        private void OnNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in monitoredItem.DequeueValues())
            {
                NlogHelper.Info($"[OPC UA 通知] {monitoredItem.DisplayName}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
                Console.WriteLine($"[通知] {monitoredItem.DisplayName}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
            }
        }

        /// <summary>
        /// 断开与指定 OPC UA 服务器的连接。
        /// </summary>
        /// <param name="endpointUrl">OPC UA 服务器的终结点 URL。</param>
        private async Task DisconnectOpcUaSession(string endpointUrl)
        {
            NlogHelper.Info($"正在断开 OPC UA 会话: {endpointUrl}");
            if (_opcUaSessions.TryGetValue(endpointUrl, out var session))
            {
                if (_opcUaSubscriptions.TryGetValue(endpointUrl, out var subscription))
                {
                    // 删除订阅。
                    subscription.Delete(true);
                    _opcUaSubscriptions.Remove(endpointUrl);
                }
                // 关闭会话。
                session.Close();
                _opcUaSessions.Remove(endpointUrl);
                NlogHelper.Info($"Disconnected from OPC UA server: {endpointUrl}");
                NotificationHelper.ShowInfo($"已从 OPC UA 服务器断开连接: {endpointUrl}");
            }
        }

        /// <summary>
        /// 断开所有 OPC UA 会话。
        /// </summary>
        private async Task DisconnectAllOpcUaSessions()
        {
            NlogHelper.Info("正在断开所有 OPC UA 会话...");
            foreach (var endpointUrl in _opcUaSessions.Keys.ToList())
            {
                await DisconnectOpcUaSession(endpointUrl);
            }
        }

        /// <summary>
        /// 处理变量数据变化的事件。
        /// 当数据库中的变量信息发生变化时，此方法被调用以重新加载和配置 OPC UA 变量。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="variableDatas">变化的变量数据列表。</param>
        private async void HandleVariableDataChanged(object sender, List<VariableData> variableDatas)
        {
            NlogHelper.Info("Variable data changed. Reloading OPC UA variables.");
            await LoadOpcUaVariables();
        }
    }
}