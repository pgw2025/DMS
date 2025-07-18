using System.Collections.Concurrent;
using DMS.Enums;
using DMS.Helper;
using DMS.Models;
using Microsoft.Extensions.Hosting;
using Opc.Ua;
using Opc.Ua.Client;

namespace DMS.Services
{
    public class OpcUaBackgroundService : BackgroundService
    {
        private readonly DataServices _dataServices;
        private readonly IDataProcessingService _dataProcessingService;

        // 存储 OPC UA 设备，键为设备Id，值为会话对象。
        private readonly ConcurrentDictionary<int, Device> _opcUaDevices;

        // 存储 OPC UA 会话，键为终结点 URL，值为会话对象。
        private readonly ConcurrentDictionary<string, Session> _opcUaSessions;

        // 存储 OPC UA 订阅，键为终结点 URL，值为订阅对象。
        private readonly ConcurrentDictionary<string, Subscription> _opcUaSubscriptions;

        // 存储活动的 OPC UA 变量，键为变量的OpcNodeId
        private readonly ConcurrentDictionary<string, Variable> _opcUaPollVariablesByNodeId;

        // 储存所有要轮询更新的变量，键是Device.Id,值是这个设备所有要轮询的变量
        private readonly ConcurrentDictionary<int, List<Variable>> _opcUaPollVariablesByDeviceId;

        // 储存所有要订阅更新的变量，键是Device.Id,值是这个设备所有要轮询的变量
        private readonly ConcurrentDictionary<int, List<Variable>> _opcUaSubVariablesByDeviceId;

        private readonly SemaphoreSlim _reloadSemaphore = new SemaphoreSlim(0);

        // OPC UA 轮询间隔（毫秒）
        private readonly int _opcUaPollIntervalMs = 100;

        // OPC UA 订阅发布间隔（毫秒）
        private readonly int _opcUaSubscriptionPublishingIntervalMs = 1000;

        // OPC UA 订阅采样间隔（毫秒）
        private readonly int _opcUaSubscriptionSamplingIntervalMs = 1000;

        public OpcUaBackgroundService(DataServices dataServices, IDataProcessingService dataProcessingService)
        {
            _dataServices = dataServices;
            _dataProcessingService = dataProcessingService;
            _opcUaDevices = new ConcurrentDictionary<int, Device>();
            _opcUaSessions = new ConcurrentDictionary<string, Session>();
            _opcUaSubscriptions = new ConcurrentDictionary<string, Subscription>();
            _opcUaPollVariablesByNodeId = new ConcurrentDictionary<string, Variable>();
            _opcUaPollVariablesByDeviceId = new ConcurrentDictionary<int, List<Variable>>();
            _opcUaSubVariablesByDeviceId = new ConcurrentDictionary<int, List<Variable>>();

            _dataServices.OnDeviceListChanged += HandleDeviceListChanged;
            _dataServices.OnDeviceIsActiveChanged += HandleDeviceIsActiveChanged;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            NlogHelper.Info("OPC UA 后台服务正在启动。");
            _reloadSemaphore.Release(); // Initial trigger to load variables and connect

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await _reloadSemaphore.WaitAsync(stoppingToken); // Wait for a reload signal

                    if (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (_dataServices.Devices == null || _dataServices.Devices.Count == 0)
                    {
                        NlogHelper.Info("没有可用的OPC UA设备，等待设备列表更新...");
                        continue;
                    }

                    var isLoaded = LoadVariables();
                    if (!isLoaded)
                    {
                        NlogHelper.Info("加载变量过程中发生了错误，停止后面的操作。");
                        continue;
                    }

                    await ConnectOpcUaServiceAsync(stoppingToken);
                    await SetupOpcUaSubscriptionAsync(stoppingToken);
                    NlogHelper.Info("OPC UA 后台服务已启动。");

                    // 持续轮询，直到取消请求或需要重新加载
                    while (!stoppingToken.IsCancellationRequested && _reloadSemaphore.CurrentCount == 0)
                    {
                        await PollOpcUaVariableOnceAsync(stoppingToken);
                        await Task.Delay(_opcUaPollIntervalMs, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                NlogHelper.Info("OPC UA 后台服务已停止。");
            }
            catch (Exception e)
            {
                NlogHelper.Error($"OPC UA 后台服务运行中发生了错误:{e.Message}", e);
            }
            finally
            {
                await DisconnectAllOpcUaSessionsAsync();
                _dataServices.OnDeviceListChanged -= HandleDeviceListChanged;
                _dataServices.OnDeviceIsActiveChanged -= HandleDeviceIsActiveChanged;
            }
        }

        private void HandleDeviceListChanged(List<Device> devices)
        {
            NlogHelper.Info("设备列表已更改。OPC UA 客户端可能需要重新初始化。");
            _reloadSemaphore.Release(); // 触发ExecuteAsync中的全面重新加载
        }

        private async void HandleDeviceIsActiveChanged(Device device, bool isActive)
        {
            if (device.ProtocolType != ProtocolType.OpcUA)
                return;

            NlogHelper.Info($"设备 {device.Name} (ID: {device.Id}) 的IsActive状态改变为 {isActive}。");

            if (!isActive)
            {
                // 设备变为非活动状态，断开连接
                if (_opcUaSessions.TryRemove(device.OpcUaEndpointUrl, out var session))
                {
                    try
                    {
                        if (_opcUaSubscriptions.TryRemove(device.OpcUaEndpointUrl, out var subscription))
                        {
                            // 删除订阅。
                            await subscription.DeleteAsync(true);
                            NlogHelper.Info($"已删除设备 {device.Name} ({device.OpcUaEndpointUrl}) 的订阅。");
                        }

                        if (session.Connected)
                        {
                            await session.CloseAsync();
                            NotificationHelper.ShowSuccess($"已断开设备 {device.Name} ({device.OpcUaEndpointUrl}) 的连接。");
                        }
                    }
                    catch (Exception ex)
                    {
                        NlogHelper.Error($"断开设备 {device.Name} ({device.OpcUaEndpointUrl}) 连接时发生错误：{ex.Message}", ex);
                    }
                }
            }

            // 触发重新加载，让LoadVariables和ConnectOpcUaServiceAsync处理设备列表的更新
            _reloadSemaphore.Release();
        }

        /// <summary>
        /// 从数据库加载所有活动的 OPC UA 变量，并进行相应的连接和订阅管理。
        /// </summary>
        private bool LoadVariables()
        {
            try
            {
                _opcUaDevices.Clear();
                _opcUaPollVariablesByDeviceId.Clear();
                _opcUaSubVariablesByDeviceId.Clear();
                _opcUaPollVariablesByNodeId.Clear();

                NlogHelper.Info("开始加载OPC UA变量....");
                var opcUaDevices = _dataServices
                                   .Devices.Where(d => d.ProtocolType == ProtocolType.OpcUA && d.IsActive == true)
                                   .ToList();

                if (opcUaDevices.Count == 0)
                {
                    NlogHelper.Info("没有找到活动的OPC UA设备。");
                    return true; // No active devices, but not an error
                }

                int totalPollVariableCount = 0;
                int totalSubVariableCount = 0;

                foreach (var opcUaDevice in opcUaDevices)
                {
                    _opcUaDevices.AddOrUpdate(opcUaDevice.Id, opcUaDevice, (key, oldValue) => opcUaDevice);

                    //查找设备中所有要轮询的变量
                    var dPollList = opcUaDevice.VariableTables?.SelectMany(vt => vt.Variables)
                                               .Where(vd => vd.IsActive == true &&
                                                            vd.ProtocolType == ProtocolType.OpcUA &&
                                                            vd.OpcUaUpdateType == OpcUaUpdateType.OpcUaPoll)
                                               .ToList();
                    // 将变量保存到字典中，方便Read后还原
                    foreach (var variable in dPollList)
                    {
                        _opcUaPollVariablesByNodeId.AddOrUpdate(variable.OpcUaNodeId, variable,
                                                            (key, oldValue) => variable);
                    }

                    totalPollVariableCount += dPollList.Count;
                    _opcUaPollVariablesByDeviceId.AddOrUpdate(opcUaDevice.Id, dPollList, (key, oldValue) => dPollList);

                    //查找设备中所有要订阅的变量
                    var dSubList = opcUaDevice.VariableTables?.SelectMany(vt => vt.Variables)
                                              .Where(vd => vd.IsActive == true &&
                                                           vd.ProtocolType == ProtocolType.OpcUA &&
                                                           vd.OpcUaUpdateType == OpcUaUpdateType.OpcUaSubscription)
                                              .ToList();
                    totalSubVariableCount += dSubList.Count;
                    _opcUaSubVariablesByDeviceId.AddOrUpdate(opcUaDevice.Id, dSubList, (key, oldValue) => dSubList);
                }

                NlogHelper.Info(
                    $"OPC UA 变量加载成功，共加载OPC UA设备：{opcUaDevices.Count}个，轮询变量数：{totalPollVariableCount}，订阅变量数：{totalSubVariableCount}");
                return true;
            }
            catch (Exception e)
            {
                NotificationHelper.ShowError($"加载OPC UA变量的过程中发生了错误：{e.Message}", e);
                return false;
            }
        }

        /// <summary>
        /// 连接到 OPC UA 服务器并订阅或轮询指定的变量。
        /// </summary>
        private async Task ConnectOpcUaServiceAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            var connectTasks = new List<Task>();

            // 遍历_opcUaDevices中的所有设备，尝试连接
            foreach (var device in _opcUaDevices.Values.ToList())
            {
                connectTasks.Add(ConnectSingleOpcUaDeviceAsync(device, stoppingToken));
            }

            await Task.WhenAll(connectTasks);
        }

        /// <summary>
        /// 连接单个OPC UA设备。
        /// </summary>
        /// <param name="device">要连接的设备。</param>
        /// <param name="stoppingToken">取消令牌。</param>
        private async Task ConnectSingleOpcUaDeviceAsync(Device device, CancellationToken stoppingToken = default)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            // Check if already connected
            if (_opcUaSessions.TryGetValue(device.OpcUaEndpointUrl, out var existingSession))
            {
                if (existingSession.Connected)
                {
                    NlogHelper.Info($"已连接到 OPC UA 服务器: {device.OpcUaEndpointUrl}");
                    return;
                }
                else
                {
                    // Remove disconnected session from dictionary to attempt reconnection
                    _opcUaSessions.TryRemove(device.OpcUaEndpointUrl, out _);
                }
            }

            NlogHelper.Info($"开始连接OPC UA服务器: {device.Name} ({device.OpcUaEndpointUrl})");
            try
            {
                var session = await ServiceHelper.CreateOpcUaSessionAsync(device.OpcUaEndpointUrl, stoppingToken);
                if (session == null)
                {
                    NlogHelper.Warn($"创建OPC UA会话失败: {device.OpcUaEndpointUrl}");
                    return; // 连接失败，直接返回
                }

                _opcUaSessions.AddOrUpdate(device.OpcUaEndpointUrl, session, (key, oldValue) => session);
                NotificationHelper.ShowSuccess($"已连接到OPC UA服务器: {device.Name} ({device.OpcUaEndpointUrl})");
            }
            catch (Exception e)
            {
                NotificationHelper.ShowError(
                    $"OPC UA服务连接 {device.Name} ({device.OpcUaEndpointUrl}) 过程中发生错误：{e.Message}", e);
            }
        }

        private async Task PollOpcUaVariableOnceAsync(CancellationToken stoppingToken)
        {
            try
            {
                var deviceIdsToPoll = _opcUaPollVariablesByDeviceId.Keys.ToList();

                var pollingTasks = deviceIdsToPoll.Select(deviceId => PollSingleDeviceVariablesAsync(deviceId, stoppingToken)).ToList();

                await Task.WhenAll(pollingTasks);
            }
            catch (OperationCanceledException)
            {
                NlogHelper.Info("OPC UA 后台服务轮询变量被取消。");
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowError($"OPC UA 后台服务在轮询变量过程中发生错误：{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 轮询单个设备的所有 OPC UA 变量。
        /// </summary>
        /// <param name="deviceId">设备的 ID。</param>
        /// <param name="stoppingToken">取消令牌。</param>
        private async Task PollSingleDeviceVariablesAsync(int deviceId, CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested) return;

            if (!_opcUaDevices.TryGetValue(deviceId, out var device) || device.OpcUaEndpointUrl == null)
            {
                NlogHelper.Warn($"OpcUa轮询变量时，在deviceDic中未找到ID为 {deviceId} 的设备，或其服务器地址为空，请检查！");
                return;
            }

            if (!device.IsActive) return;

            if (!_opcUaSessions.TryGetValue(device.OpcUaEndpointUrl, out var session) || !session.Connected)
            {
                if (device.IsActive)
                {
                    NlogHelper.Warn($"用于 {device.OpcUaEndpointUrl} 的 OPC UA 会话未连接。正在尝试重新连接...");
                    await ConnectSingleOpcUaDeviceAsync(device, stoppingToken);
                }
                return;
            }

            if (!_opcUaPollVariablesByDeviceId.TryGetValue(deviceId, out var variableList) || variableList.Count == 0)
            {
                return;
            }

            foreach (var variable in variableList)
            {
                if (stoppingToken.IsCancellationRequested) return;

                if (!ServiceHelper.PollingIntervals.TryGetValue(variable.PollLevelType, out var interval) || (DateTime.Now - variable.UpdateTime) < interval)
                {
                    continue;
                }

                await ReadAndProcessOpcUaVariableAsync(session, variable, stoppingToken);
            }
        }

        /// <summary>
        /// 读取单个 OPC UA 变量并处理其数据。
        /// </summary>
        /// <param name="session">OPC UA 会话。</param>
        /// <param name="variable">要读取的变量。</param>
        /// <param name="stoppingToken">取消令牌。</param>
        private async Task ReadAndProcessOpcUaVariableAsync(Session session, Variable variable, CancellationToken stoppingToken)
        {
            var nodesToRead = new ReadValueIdCollection
            {
                new ReadValueId
                {
                    NodeId = new NodeId(variable.OpcUaNodeId),
                    AttributeId = Attributes.Value
                }
            };

            try
            {
                var readResponse = await session.ReadAsync(null, 0, TimestampsToReturn.Both, nodesToRead, stoppingToken);
                var result = readResponse.Results?.FirstOrDefault();
                if (result == null) return;

                if (!StatusCode.IsGood(result.StatusCode))
                {
                    NlogHelper.Warn($"读取 OPC UA 变量 {variable.Name} ({variable.OpcUaNodeId}) 失败: {result.StatusCode}");
                    return;
                }

                await UpdateAndEnqueueVariable(variable, result.Value);
            }
            catch (ServiceResultException ex) when (ex.StatusCode == StatusCodes.BadSessionIdInvalid)
            {
                NlogHelper.Error($"OPC UA会话ID无效，变量: {variable.Name} ({variable.OpcUaNodeId})。正在尝试重新连接...", ex);
                // Assuming device can be retrieved from variable or passed as parameter if needed for ConnectSingleOpcUaDeviceAsync
                // For now, I'll just log and let the outer loop handle reconnection if the session is truly invalid for the device.
                // If a full device object is needed here, it would need to be passed down from PollSingleDeviceVariablesAsync.
                // For simplicity, I'll remove the direct reconnection attempt here and rely on the outer loop.
                await ConnectSingleOpcUaDeviceAsync(variable.VariableTable.Device, stoppingToken);
            }
            catch (Exception ex)
            {
                NlogHelper.Error($"轮询OPC UA变量 {variable.Name} ({variable.OpcUaNodeId}) 时发生未知错误: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新变量数据，并将其推送到数据处理队列。
        /// </summary>
        /// <param name="variable">要更新的变量。</param>
        /// <param name="value">读取到的数据值。</param>
        private async Task UpdateAndEnqueueVariable(Variable variable, object value)
        {
            try
            {
                // 更新变量的原始数据值和显示值。
                variable.DataValue = value.ToString();
                variable.DisplayValue = value.ToString(); // 或者根据需要进行格式化
                variable.UpdateTime = DateTime.Now;
                // Console.WriteLine($"OpcUa后台服务轮询变量：{variable.Name},值：{variable.DataValue}");
                // 将更新后的数据推入处理队列。
                await _dataProcessingService.EnqueueAsync(variable);
            }
            catch (Exception ex)
            {
                NlogHelper.Error($"更新变量 {variable.Name} 并入队失败:{ex.Message}", ex);
            }
        }

        /// <summary>
        /// 设置 OPC UA 订阅并添加监控项。
        /// </summary>
        /// <param name="stoppingToken">取消令牌。</param>
        private async Task SetupOpcUaSubscriptionAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            var setupSubscriptionTasks = new List<Task>();

            foreach (var deviceId in _opcUaSubVariablesByDeviceId.Keys.ToList())
            {
                setupSubscriptionTasks.Add(Task.Run(() =>
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        return; // 任务被取消，退出循环
                    }

                    var device = _dataServices.Devices.FirstOrDefault(d => d.Id == deviceId);
                    if (device == null)
                    {
                        NlogHelper.Warn($"未找到ID为 {deviceId} 的设备，无法设置订阅。");
                        return;
                    }

                    Subscription subscription = null;
                    // 得到session
                    if (!_opcUaSessions.TryGetValue(device.OpcUaEndpointUrl, out var session))
                    {
                        NlogHelper.Info($"从OpcUa会话字典中获取会话失败： {device.OpcUaEndpointUrl} ");
                        return;
                    }

                    // 判断设备是否已经添加了订阅
                    if (_opcUaSubscriptions.TryGetValue(device.OpcUaEndpointUrl, out subscription))
                    {
                        NlogHelper.Info($"OPC UA 终结点 {device.OpcUaEndpointUrl} 已存在订阅。");
                    }
                    else
                    {
                        subscription = new Subscription(session.DefaultSubscription);
                        subscription.PublishingInterval = _opcUaSubscriptionPublishingIntervalMs; // 发布间隔（毫秒）
                        session.AddSubscription(subscription);
                        subscription.Create();
                        _opcUaSubscriptions.AddOrUpdate(device.OpcUaEndpointUrl, subscription,
                                                      (key, oldValue) => subscription);
                    }

                    // 将变量添加到订阅
                    if (_opcUaSubVariablesByDeviceId.TryGetValue(deviceId, out var variablesToSubscribe))
                    {
                        foreach (Variable variable in variablesToSubscribe)
                        {
                            // 7. 创建监控项并添加到订阅中。
                            MonitoredItem monitoredItem = new MonitoredItem(subscription.DefaultItem);
                            monitoredItem.DisplayName = variable.Name;
                            monitoredItem.StartNodeId = new NodeId(variable.OpcUaNodeId); // 设置要监控的节点 ID
                            monitoredItem.AttributeId = Attributes.Value; // 监控节点的值属性
                            monitoredItem.SamplingInterval = _opcUaSubscriptionSamplingIntervalMs; // 采样间隔（毫秒）
                            monitoredItem.QueueSize = 1; // 队列大小
                            monitoredItem.DiscardOldest = true; // 丢弃最旧的数据
                            // 注册数据变化通知事件。
                            monitoredItem.Notification += (sender, e) => OnSubNotification(variable,sender, e);

                            subscription.AddItem(monitoredItem);
                        }

                        subscription.ApplyChanges(); // 应用更改
                        NlogHelper.Info($"设备: {device.Name}, 添加了 {variablesToSubscribe.Count} 个订阅变量。");
                    }
                }));
            }

            await Task.WhenAll(setupSubscriptionTasks);
        }

        /// <summary>
        /// 订阅变量变化的通知
        /// </summary>
        /// <param name="variable">发生变化的变量。</param>
        /// <param name="monitoredItem"></param>
        /// <param name="e"></param>
        private async void OnSubNotification(Variable variable, MonitoredItem monitoredItem,
                                             MonitoredItemNotificationEventArgs e)
        {
            
            foreach (var value in monitoredItem.DequeueValues())
            {
                Console.WriteLine(
                    $"[OPC UA 通知] {monitoredItem.DisplayName}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
                if (StatusCode.IsGood(value.StatusCode))
                {
                    await UpdateAndEnqueueVariable(variable, value.Value);
                }
            }
        }

        /// <summary>
        /// 断开所有 OPC UA 会话。
        /// </summary>
        private async Task DisconnectAllOpcUaSessionsAsync()
        {
            if (_opcUaSessions.IsEmpty)
                return;

            NlogHelper.Info("正在断开所有 OPC UA 会话...");
            var closeTasks = new List<Task>();

            foreach (var endpointUrl in _opcUaSessions.Keys.ToList())
            {
                closeTasks.Add(Task.Run(async () =>
                {
                    NlogHelper.Info($"正在断开 OPC UA 会话: {endpointUrl}");
                    if (_opcUaSessions.TryRemove(endpointUrl, out var session))
                    {
                        if (_opcUaSubscriptions.TryRemove(endpointUrl, out var subscription))
                        {
                            // 删除订阅。
                            await subscription.DeleteAsync(true);
                        }

                        // 关闭会话。
                        await session.CloseAsync();
                        NotificationHelper.ShowInfo($"已从 OPC UA 服务器断开连接: {endpointUrl}");
                    }
                }));
            }

            await Task.WhenAll(closeTasks);
        }
    }
}