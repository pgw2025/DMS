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
        private readonly Dictionary<int, CancellationTokenSource> _opcUaPollingTasks; // Key: VariableData.Id, Value: CancellationTokenSource for polling task

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
            _opcUaPollingTasks = new Dictionary<int, CancellationTokenSource>();
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
                NlogHelper.Info("OpcUaBackgroundService stopping.");
                // 服务停止时，取消订阅事件并断开所有 OPC UA 连接。
                _dataServices.OnVariableDataChanged -= HandleVariableDataChanged;
                await DisconnectAllOpcUaSessions();
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
            // while (!stoppingToken.IsCancellationRequested)
            // {
            //     // 可以在这里添加周期性任务，例如检查和重新连接断开的会话。
            //     await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            // }

           
        }

        /// <summary>
        /// 从数据库加载所有活动的 OPC UA 变量，并进行相应的连接和订阅管理。
        /// </summary>
        private async Task LoadOpcUaVariables()
        {
            NlogHelper.Info("正在加载 OPC UA 变量...");
            // var allVariables = await _dataServices.GetAllVariableDataAsync();
            var opcUaVariables = _dataServices.VariableDatas.Where(v => v.ProtocolType == ProtocolType.OpcUA && v.IsActive).ToList();
            
            if (opcUaVariables==null || opcUaVariables.Count==0)
                return;

            // 清理不再活跃或已删除的变量。
            await RemoveInactiveOpcUaVariables(opcUaVariables);

            // 处理新增或更新的变量。
            await ProcessActiveOpcUaVariables(opcUaVariables);
        }

        /// <summary>
        /// 连接到 OPC UA 服务器并订阅或轮询指定的变量。
        /// </summary>
        /// <param name="variable">要订阅或轮询的变量信息。</param>
        /// <param name="device">变量所属的设备信息。</param>
        private async Task ConnectAndSubscribeOpcUa(VariableData variable, Device device)
        {
            NlogHelper.Info($"正在为变量 '{variable.Name}' 连接和处理 OPC UA 服务器... (更新类型: {variable.OpcUaUpdateType})");
            if (string.IsNullOrEmpty(device.OpcUaEndpointUrl) || string.IsNullOrEmpty(variable.OpcUaNodeId))
            {
                NlogHelper.Warn($"OPC UA variable {variable.Name} has invalid EndpointUrl or NodeId.");
                return;
            }

            Session session = null;
            // 检查是否已存在到该终结点的活动会话。
            if (_opcUaSessions.TryGetValue(device.OpcUaEndpointUrl, out session) && session.Connected)
            {
                NlogHelper.Info($"Already connected to OPC UA endpoint: {device.OpcUaEndpointUrl}");
            }
            else
            {
                session = await CreateOpcUaSessionAsync(device.OpcUaEndpointUrl);
                if (session == null)
                {
                    return; // 连接失败，直接返回
                }
            }

            if (variable.OpcUaUpdateType == OpcUaUpdateType.OpcUaSubscription)
            {
                SetupOpcUaSubscription(session, variable, device.OpcUaEndpointUrl);
            }
            else if (variable.OpcUaUpdateType == OpcUaUpdateType.OpcUaPoll)
            {
                StartOpcUaPolling(session, variable, device);
            }
        }

        private async Task PollOpcUaVariable(Session session, VariableData variable, Device device, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (session != null && session.Connected)
                    {
                        var nodeToRead = new ReadValueId
                        {
                            NodeId = new NodeId(variable.OpcUaNodeId),
                            AttributeId = Attributes.Value
                        };

                        var nodesToRead = new ReadValueIdCollection { nodeToRead };
                        session.Read(
                            null,
                            0,
                            TimestampsToReturn.Both,
                            nodesToRead,
                            out DataValueCollection results,
                            out DiagnosticInfoCollection diagnosticInfos);

                        if (results != null && results.Count > 0)
                        {
                            var value = results[0];
                            if (StatusCode.IsGood(value.StatusCode))
                            {
                                NlogHelper.Info($"[OPC UA 轮询] {variable.Name}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
                                // 更新变量数据
                                variable.DataValue = value.Value.ToString();
                                variable.DisplayValue = value.Value.ToString(); // 或者根据需要进行格式化
                                // await _dataServices.UpdateVariableDataAsync(variable);
                            }
                            else
                            {
                                NlogHelper.Warn($"Failed to read OPC UA variable {variable.Name} ({variable.OpcUaNodeId}): {value.StatusCode}");
                            }
                        }
                    }
                    else
                    {
                        NlogHelper.Warn($"OPC UA session for {device.OpcUaEndpointUrl} is not connected. Attempting to reconnect...");
                        // 尝试重新连接会话
                        await ConnectAndSubscribeOpcUa(variable, device);
                    }
                }
                catch (Exception ex)
                {
                    NlogHelper.Error($"Error during OPC UA polling for {variable.Name}: {ex.Message}", ex);
                }

                // 根据 PollLevelType 设置轮询间隔
                var pollInterval = GetPollInterval(variable.PollLevelType);
                await Task.Delay(pollInterval, cancellationToken);
            }
            NlogHelper.Info($"Polling for OPC UA variable {variable.Name} stopped.");
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
        /// 根据 PollLevelType 获取轮询间隔。
        /// </summary>
        /// <param name="pollLevelType">轮询级别类型。</param>
        /// <returns>时间间隔。</returns>
        private TimeSpan GetPollInterval(PollLevelType pollLevelType)
        {
            return pollLevelType switch
            {
                PollLevelType.OneSecond => TimeSpan.FromSeconds(1),
                PollLevelType.FiveSeconds => TimeSpan.FromSeconds(5),
                PollLevelType.TenSeconds => TimeSpan.FromSeconds(10),
                PollLevelType.ThirtySeconds => TimeSpan.FromSeconds(30),
                PollLevelType.OneMinute => TimeSpan.FromMinutes(1),
                PollLevelType.FiveMinutes => TimeSpan.FromMinutes(5),
                PollLevelType.TenMinutes => TimeSpan.FromMinutes(10),
                PollLevelType.ThirtyMinutes => TimeSpan.FromMinutes(30),
                PollLevelType.OneHour => TimeSpan.FromHours(1),
                _ => TimeSpan.FromSeconds(1), // 默认1秒
            };
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

                // 取消与此会话相关的轮询任务
                var variablesToCancelPolling = _opcUaVariables.Where(kv => kv.Value.VariableTable != null && kv.Value.VariableTable.DeviceId.HasValue && _dataServices.GetDeviceByIdAsync(kv.Value.VariableTable.DeviceId.Value).Result?.OpcUaEndpointUrl == endpointUrl && kv.Value.OpcUaUpdateType == OpcUaUpdateType.OpcUaPoll).ToList();
                foreach (var entry in variablesToCancelPolling)
                {
                    if (_opcUaPollingTasks.ContainsKey(entry.Key))
                    {
                        var cts = _opcUaPollingTasks[entry.Key];
                        _opcUaPollingTasks.Remove(entry.Key);
                        cts.Cancel();
                        cts.Dispose();
                        NlogHelper.Info($"Cancelled polling for variable: {entry.Value.Name} (ID: {entry.Key})");
                    }
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
        private async void HandleVariableDataChanged( List<VariableData> variableDatas)
        {
            NlogHelper.Info("Variable data changed. Reloading OPC UA variables.");
            await LoadOpcUaVariables();
        }

        /// <summary>
        /// 创建并配置 OPC UA 会话。
        /// </summary>
        /// <param name="endpointUrl">OPC UA 服务器的终结点 URL。</param>
        /// <returns>创建的 Session 对象，如果失败则返回 null。</returns>
        private async Task<Session> CreateOpcUaSessionAsync(string endpointUrl)
        {
            try
            {
                // 1. 创建应用程序配置
                var application = new ApplicationInstance
                {
                    ApplicationName = "OpcUADemoClient",
                    ApplicationType = ApplicationType.Client,
                    ConfigSectionName = "Opc.Ua.Client"
                };

                var config = new ApplicationConfiguration()
                {
                    ApplicationName = application.ApplicationName,
                    ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:OpcUADemoClient",
                    ApplicationType = application.ApplicationType,
                    SecurityConfiguration = new SecurityConfiguration
                    {
                        ApplicationCertificate = new CertificateIdentifier
                        {
                            StoreType = "Directory",
                            StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/MachineDefault",
                            SubjectName = application.ApplicationName
                        },
                        TrustedIssuerCertificates = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/UA Certificate Authorities"
                        },
                        TrustedPeerCertificates = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/UA Applications"
                        },
                        RejectedCertificateStore = new CertificateTrustList
                        {
                            StoreType = "Directory",
                            StorePath = "%CommonApplicationData%/OPC Foundation/CertificateStores/RejectedCertificates"
                        },
                        AutoAcceptUntrustedCertificates = true // 自动接受不受信任的证书 (仅用于测试)
                    },
                    TransportQuotas = new TransportQuotas { OperationTimeout = 15000 },
                    ClientConfiguration = new ClientConfiguration { DefaultSessionTimeout = 60000 },
                    TraceConfiguration = new TraceConfiguration
                    {
                        OutputFilePath = "./Logs/OpcUaClient.log", DeleteOnLoad = true,
                        TraceMasks = Utils.TraceMasks.Error | Utils.TraceMasks.Security
                    }
                };
                application.ApplicationConfiguration = config;

                // 验证并检查证书
                await config.Validate(ApplicationType.Client);
                await application.CheckApplicationInstanceCertificate(false, 0);

                // 2. 查找并选择端点 (将 useSecurity 设置为 false 以进行诊断)
                var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointUrl, false);

                var session = await Session.Create(
                    config,
                    new ConfiguredEndpoint(null, selectedEndpoint, EndpointConfiguration.Create(config)),
                    false,
                    "PMSWPF OPC UA Session",
                    60000,
                    new UserIdentity(new AnonymousIdentityToken()),
                    null);

                _opcUaSessions[endpointUrl] = session;
                NotificationHelper.ShowSuccess($"已连接到 OPC UA 服务器: {endpointUrl}");
                return session;
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowError($"连接 OPC UA 服务器失败: {endpointUrl} - {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// 设置 OPC UA 订阅并添加监控项。
        /// </summary>
        /// <param name="session">OPC UA 会话。</param>
        /// <param name="variable">要订阅的变量信息。</param>
        /// <param name="endpointUrl">OPC UA 服务器的终结点 URL。</param>
        private void SetupOpcUaSubscription(Session session, VariableData variable, string endpointUrl)
        {
            Subscription subscription = null;
            if (_opcUaSubscriptions.TryGetValue(endpointUrl, out subscription))
            {
                NlogHelper.Info($"Already has subscription for OPC UA endpoint: {endpointUrl}");
            }
            else
            {
                subscription = new Subscription(session.DefaultSubscription);
                subscription.PublishingInterval = 1000; // 发布间隔（毫秒）
                session.AddSubscription(subscription);
                subscription.Create();
                _opcUaSubscriptions[endpointUrl] = subscription;
            }

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
            NlogHelper.Info($"Subscribed to OPC UA variable: {variable.Name} ({variable.OpcUaNodeId})");
        }

        /// <summary>
        /// 启动 OPC UA 变量的轮询任务。
        /// </summary>
        /// <param name="session">OPC UA 会话。</param>
        /// <param name="variable">要轮询的变量信息。</param>
        /// <param name="device">变量所属的设备信息。</param>
        private void StartOpcUaPolling(Session session, VariableData variable, Device device)
        {
            if (!_opcUaPollingTasks.ContainsKey(variable.Id))
            {
                var cts = new CancellationTokenSource();
                _opcUaPollingTasks.Add(variable.Id, cts);
                _ = Task.Run(() => PollOpcUaVariable(session, variable, device, cts.Token), cts.Token);
                NlogHelper.Info($"Started polling for OPC UA variable: {variable.Name} ({variable.OpcUaNodeId})");
            }
        }

        /// <summary>
        /// 移除不再活跃或已删除的 OPC UA 变量。
        /// </summary>
        /// <param name="activeOpcUaVariables">当前所有活跃的 OPC UA 变量列表。</param>
        private async Task RemoveInactiveOpcUaVariables(List<VariableData> activeOpcUaVariables)
        {
            var currentOpcUaVariableIds = activeOpcUaVariables.Select(v => v.Id).ToHashSet();
            var variablesToRemove = _opcUaVariables.Keys.Except(currentOpcUaVariableIds).ToList();

            NlogHelper.Info($"发现 {variablesToRemove.Count} 个要移除的 OPC UA 变量。");

            foreach (var id in variablesToRemove)
            {
                if (_opcUaVariables.TryGetValue(id, out var variable))
                {
                    if (variable.OpcUaUpdateType == OpcUaUpdateType.OpcUaSubscription)
                    {
                        // 获取关联的设备信息
                        var device = await _dataServices.GetDeviceByIdAsync(variable.VariableTable.DeviceId??0);
                        if (device != null)
                        {
                            // 断开与该变量相关的 OPC UA 会话。
                            await DisconnectOpcUaSession(device.OpcUaEndpointUrl);
                        }
                    }
                    else if (variable.OpcUaUpdateType == OpcUaUpdateType.OpcUaPoll)
                    {
                        if (_opcUaPollingTasks.ContainsKey(variable.Id))
                        {
                            var cts = _opcUaPollingTasks[variable.Id];
                            _opcUaPollingTasks.Remove(variable.Id);
                            cts.Cancel();
                            cts.Dispose();
                        }
                    }
                    _opcUaVariables.Remove(id);
                }
            }
        }

        /// <summary>
        /// 处理新增或更新的活跃 OPC UA 变量。
        /// </summary>
        /// <param name="opcUaVariables">当前所有活跃的 OPC UA 变量列表。</param>
        private async Task ProcessActiveOpcUaVariables(List<VariableData> opcUaVariables)
        {
            foreach (var variable in opcUaVariables)
            {
                // 获取关联的设备信息
                var device = await _dataServices.GetDeviceByIdAsync(variable.VariableTable.DeviceId??0);
                if (device == null)
                {
                    NlogHelper.Warn($"变量 '{variable.Name}' (ID: {variable.Id}) 关联的设备不存在。");
                    continue;
                }

                if (!_opcUaVariables.ContainsKey(variable.Id))
                {
                    // 如果是新变量，则添加到字典并建立连接和订阅。
                    _opcUaVariables.Add(variable.Id, variable);
                    await ConnectAndSubscribeOpcUa(variable, device);
                }
                else
                {
                    // 如果变量已存在，则更新其信息。
                    _opcUaVariables[variable.Id] = variable;
                    // 如果终结点 URL 对应的会话已断开，则尝试重新连接。
                    if (_opcUaSessions.ContainsKey(device.OpcUaEndpointUrl) && !_opcUaSessions[device.OpcUaEndpointUrl].Connected)
                    {
                        await ConnectAndSubscribeOpcUa(variable, device);
                    }
                }
            }
        }
    }
}