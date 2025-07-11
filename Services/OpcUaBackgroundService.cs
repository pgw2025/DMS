using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    ///     - 遍历所有活动的 OPC UA 变量，为新增的变量调用 `ConnectOpcUaService` 方法建立连接和订阅。
    ///     - 如果变量已存在但会话断开，则尝试重新连接。
    ///
    /// 4.  **连接与订阅 (`ConnectOpcUaService`)**:
    ///     - 检查变量是否包含有效的终结点 URL 和节点 ID。
    ///     - 如果到目标终结点的会话已存在且已连接，则复用该会话。
    ///     - 否则，创建一个新的 OPC UA `Session`，并将其缓存到 `_opcUaSessions` 字典中。
    ///     - 创建一个 `Subscription`，并将其缓存到 `_opcUaSubscriptions` 字典中。
    ///     - 创建一个 `MonitoredItem` 来监控指定变量节点的值变化，并将其添加到订阅中。
    ///     - 注册 `OnSubNotification` 事件回调，用于处理接收到的数据更新。
    ///
    /// 5.  **数据接收 (`OnSubNotification`)**:
    ///     - 当订阅的变量值发生变化时，OPC UA 服务器会发送通知。
    ///     - `OnSubNotification` 方法被触发，处理接收到的数据。
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

        // 储存所有要轮询更新的变量，键是Device.Id,值是这个设备所有要轮询的变量
        private readonly Dictionary<int, List<VariableData>> _opcUaPollVariableDic; // Key: VariableData.Id

        // 储存所有要订阅更新的变量，键是Device.Id,值是这个设备所有要轮询的变量
        private readonly Dictionary<int, List<VariableData>> _opcUaSubVariableDic;

        // private readonly Dictionary<int, CancellationTokenSource>
        //     _opcUaPollingTasks; // Key: VariableData.Id, Value: CancellationTokenSource for polling task

        private List<Device> _opcUaDevices;
        
        // 定义不同轮询级别的间隔时间。
        private readonly Dictionary<PollLevelType, TimeSpan> _pollingIntervals = new Dictionary<PollLevelType, TimeSpan>
            {
                { PollLevelType.TenMilliseconds, TimeSpan.FromMilliseconds((int)PollLevelType.TenMilliseconds) },
                {
                    PollLevelType.HundredMilliseconds, TimeSpan.FromMilliseconds((int)PollLevelType.HundredMilliseconds)
                },
                {
                    PollLevelType.FiveHundredMilliseconds,
                    TimeSpan.FromMilliseconds((int)PollLevelType.FiveHundredMilliseconds)
                },
                { PollLevelType.OneSecond, TimeSpan.FromMilliseconds((int)PollLevelType.OneSecond) },
                { PollLevelType.FiveSeconds, TimeSpan.FromMilliseconds((int)PollLevelType.FiveSeconds) },
                { PollLevelType.TenSeconds, TimeSpan.FromMilliseconds((int)PollLevelType.TenSeconds) },
                { PollLevelType.TwentySeconds, TimeSpan.FromMilliseconds((int)PollLevelType.TwentySeconds) },
                { PollLevelType.ThirtySeconds, TimeSpan.FromMilliseconds((int)PollLevelType.ThirtySeconds) },
                { PollLevelType.OneMinute, TimeSpan.FromMilliseconds((int)PollLevelType.OneMinute) },
                { PollLevelType.ThreeMinutes, TimeSpan.FromMilliseconds((int)PollLevelType.ThreeMinutes) },
                { PollLevelType.FiveMinutes, TimeSpan.FromMilliseconds((int)PollLevelType.FiveMinutes) },
                { PollLevelType.TenMinutes, TimeSpan.FromMilliseconds((int)PollLevelType.TenMinutes) },
                { PollLevelType.ThirtyMinutes, TimeSpan.FromMilliseconds((int)PollLevelType.ThirtyMinutes) }
            };

        /// <summary>
        /// OpcUaBackgroundService 的构造函数。
        /// </summary>
        /// <param name="dataServices">数据服务，用于访问数据库中的变量信息。</param>
        public OpcUaBackgroundService(DataServices dataServices)
        {
            _dataServices = dataServices;
            _opcUaSessions = new Dictionary<string, Session>();
            _opcUaSubscriptions = new Dictionary<string, Subscription>();
            _opcUaVariables = new();
            _opcUaPollVariableDic = new();
            _opcUaSubVariableDic = new();
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
            // while (!stoppingToken.IsCancellationRequested)
            // {
            //     // 可以在这里添加周期性任务，例如检查和重新连接断开的会话。
            //     await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            // }
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
            // var allVariables = await _dataServices.GetAllVariableDataAsync();
            _opcUaDevices = _dataServices.Devices.Where(d => d.ProtocolType == ProtocolType.OpcUA && d.IsActive == true)
                                         .ToList();
            if (_opcUaDevices.Count == 0)
                return;
            foreach (var opcUaDevice in _opcUaDevices)
            {
                //查找设备中所有要轮询的变量
                var dPollList = opcUaDevice?.VariableTables?.SelectMany(vt => vt.DataVariables)
                                           .Where(vd => vd.IsActive == true && vd.ProtocolType == ProtocolType.OpcUA &&
                                                        vd.OpcUaUpdateType == OpcUaUpdateType.OpcUaPoll)
                                           .ToList();
                _opcUaPollVariableDic.Add(opcUaDevice.Id, dPollList);
                //查找设备中所有要订阅的变量
                var dSubList = opcUaDevice?.VariableTables?.SelectMany(vt => vt.DataVariables)
                                          .Where(vd => vd.IsActive == true && vd.ProtocolType == ProtocolType.OpcUA &&
                                                       vd.OpcUaUpdateType == OpcUaUpdateType.OpcUaSubscription)
                                          .ToList();
                _opcUaSubVariableDic.Add(opcUaDevice.Id, dSubList);
            }

            if (_opcUaSubVariableDic.Count == 0 && _opcUaPollVariableDic.Count == 0)
                return;

            //连接服务器
            await ConnectOpcUaService();
            // 添加订阅变量
            SetupOpcUaSubscription();

            await PollOpcUaVariable();
        }

        /// <summary>
        /// 连接到 OPC UA 服务器并订阅或轮询指定的变量。
        /// </summary>
        /// <param name="variable">要订阅或轮询的变量信息。</param>
        /// <param name="device">变量所属的设备信息。</param>
        private async Task ConnectOpcUaService()
        {
            foreach (Device device in _opcUaDevices)
            {
                Session session = null;
                // 检查是否已存在到该终结点的活动会话。
                if (_opcUaSessions.TryGetValue(device.OpcUaEndpointUrl, out session) && session.Connected)
                {
                    NlogHelper.Info($"Already connected to OPC UA endpoint: {device.OpcUaEndpointUrl}");
                }
                else
                {
                    session = await OpcUaServiceHelper.CreateOpcUaSessionAsync(device.OpcUaEndpointUrl);
                    if (session == null)
                        return; // 连接失败，直接返回

                    _opcUaSessions[device.OpcUaEndpointUrl] = session;
                }
            }
        }

        private async Task PollOpcUaVariable()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    foreach (var deviceId in _opcUaPollVariableDic.Keys)
                    {
                        await Task.Delay(100);
                        var device = _dataServices.Devices.FirstOrDefault(d => d.Id == deviceId);
                        if (device == null || device.OpcUaEndpointUrl == String.Empty)
                        {
                            NlogHelper.Warn(
                                $"OpcUa轮询变量，在DataService中没有找到Id为：{deviceId},的设备,或者服务器地址:{device.OpcUaEndpointUrl} 为空，请检查！！");
                            continue;
                        }

                        _opcUaSessions.TryGetValue(device.OpcUaEndpointUrl, out Session session);
                        if (session == null || !session.Connected)
                        {
                            NlogHelper.Warn(
                                $"OPC UA session for {device.OpcUaEndpointUrl} is not connected. Attempting to reconnect...");
                            // 尝试重新连接会话
                            await ConnectOpcUaService();
                            continue;
                        }

                        var nodesToRead = new ReadValueIdCollection();
                        var variableList = _opcUaPollVariableDic[deviceId];
                        foreach (var variable in variableList)
                        {
                            // 获取变量的轮询间隔。
                            if (!_pollingIntervals.TryGetValue(variable.PollLevelType, out var interval))
                            {
                                NlogHelper.Info($"未知轮询级别 {variable.PollLevelType}，跳过变量 {variable.Name}。");
                                continue;
                            }
                            // 检查是否达到轮询时间。
                            if ((DateTime.Now - variable.UpdateTime) < interval)
                            {
                                continue; // 未到轮询时间，跳过。
                            }
                            
                            
                            nodesToRead.Add(new ReadValueId
                                            {
                                                NodeId = new NodeId(variable.OpcUaNodeId),
                                                AttributeId = Attributes.Value
                                            });
                        }
                        // 如果没有要读取的变量则跳过
                        if (nodesToRead.Count == 0)
                        {
                            continue;
                        }

                        session.Read(
                            null,
                            0,
                            TimestampsToReturn.Both,
                            nodesToRead,
                            out DataValueCollection results,
                            out DiagnosticInfoCollection diagnosticInfos);

                        if (results != null && results.Count > 0)
                        {
                            for (int i = 0; i < results.Count; i++)
                            {
                                var value = results[i];
                                var variable = variableList[i];
                                if (StatusCode.IsGood(value.StatusCode))
                                {
                                    NlogHelper.Info(
                                    $"[OPC UA 轮询] {variable.Name}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
                                    // 更新变量数据
                                    variable.DataValue = value.Value.ToString();
                                    variable.DisplayValue = value.Value.ToString(); // 或者根据需要进行格式化
                                    variable.UpdateTime=DateTime.Now;
                                    // await _dataServices.UpdateVariableDataAsync(variable);
                                }
                                else
                                {
                                    NlogHelper.Warn(
                                        $"Failed to read OPC UA variable {variable.Name} ({variable.OpcUaNodeId}): {value.StatusCode}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    NlogHelper.Error($"Error during OPC UA polling for  {ex.Message}", ex);
                }
            }

            NlogHelper.Info($"Polling for OPC UA variable stopped.");
        }

        /// <summary>
        /// 订阅变量变化的通知
        /// </summary>
        /// <param name="monitoredItem"></param>
        /// <param name="e"></param>
        private void OnSubNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in monitoredItem.DequeueValues())
            {
                NlogHelper.Info(
                    $"[OPC UA 通知] {monitoredItem.DisplayName}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
                Console.WriteLine(
                    $"[通知] {monitoredItem.DisplayName}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
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
        private async void HandleVariableDataChanged(List<VariableData> variableDatas)
        {
            NlogHelper.Info("Variable data changed. Reloading OPC UA variables.");
            await LoadOpcUaVariables();
        }


        /// <summary>
        /// 设置 OPC UA 订阅并添加监控项。
        /// </summary>
        /// <param name="session">OPC UA 会话。</param>
        /// <param name="variable">要订阅的变量信息。</param>
        /// <param name="endpointUrl">OPC UA 服务器的终结点 URL。</param>
        private void SetupOpcUaSubscription()
        {
            foreach (var deviceId in _opcUaSubVariableDic.Keys)
            {
                var device = _dataServices.Devices.FirstOrDefault(d => d.Id == deviceId);
                Subscription subscription = null;
                // 得到session
                _opcUaSessions.TryGetValue(device.OpcUaEndpointUrl, out var session);

                // 判断设备是否已经添加了订阅
                if (_opcUaSubscriptions.TryGetValue(device.OpcUaEndpointUrl, out subscription))
                {
                    NlogHelper.Info($"Already has subscription for OPC UA endpoint: {device.OpcUaEndpointUrl}");
                }
                else
                {
                    subscription = new Subscription(session.DefaultSubscription);
                    subscription.PublishingInterval = 1000; // 发布间隔（毫秒）
                    session.AddSubscription(subscription);
                    subscription.Create();
                    _opcUaSubscriptions[device.OpcUaEndpointUrl] = subscription;
                }

                // 将变量添加到订阅
                foreach (VariableData variable in _opcUaSubVariableDic[deviceId])
                {
                    // 7. 创建监控项并添加到订阅中。
                    MonitoredItem monitoredItem = new MonitoredItem(subscription.DefaultItem);
                    monitoredItem.DisplayName = variable.Name;
                    monitoredItem.StartNodeId = new NodeId(variable.OpcUaNodeId); // 设置要监控的节点 ID
                    monitoredItem.AttributeId = Attributes.Value; // 监控节点的值属性
                    monitoredItem.SamplingInterval = 1000; // 采样间隔（毫秒）
                    monitoredItem.QueueSize = 1; // 队列大小
                    monitoredItem.DiscardOldest = true; // 丢弃最旧的数据
                    // 注册数据变化通知事件。
                    monitoredItem.Notification += OnSubNotification;

                    subscription.AddItem(monitoredItem);
                    subscription.ApplyChanges(); // 应用更改
                }

                NlogHelper.Info($"设备: {device.Name},添加订阅变量{_opcUaSubVariableDic[deviceId]?.Count ?? 0} 个");
            }
        }
    }
}