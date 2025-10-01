using System.Collections.Concurrent;
using System.Diagnostics;
using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Models;
using DMS.Core.Enums;
using DMS.Core.Events;
using DMS.Infrastructure.Configuration;
using DMS.Infrastructure.Interfaces.Services;
using DMS.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DMS.Infrastructure.Services.OpcUa
{
    /// <summary>
    /// OPC UA服务管理器，负责管理OPC UA连接、订阅和变量监控
    /// </summary>
    public class OpcUaServiceManager : IOpcUaServiceManager
    {
        private readonly ILogger<OpcUaServiceManager> _logger;
        private readonly IDataProcessingService _dataProcessingService;
        private readonly IAppDataCenterService _appDataCenterService;
        private readonly IEventService _eventService;
        private readonly OpcUaServiceOptions _options;
        private readonly ConcurrentDictionary<int, DeviceContext> _deviceContexts;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;

        public OpcUaServiceManager(
            ILogger<OpcUaServiceManager> logger,
            IDataProcessingService dataProcessingService,
            IEventService eventService,
            IAppDataCenterService appDataCenterService,
            IOptions<OpcUaServiceOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataProcessingService
                = dataProcessingService ?? throw new ArgumentNullException(nameof(dataProcessingService));
            _eventService = eventService;
            _appDataCenterService
                = appDataCenterService ?? throw new ArgumentNullException(nameof(appDataCenterService));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _deviceContexts = new ConcurrentDictionary<int, DeviceContext>();
            _semaphore = new SemaphoreSlim(_options.MaxConcurrentConnections, _options.MaxConcurrentConnections);

            _eventService.OnDeviceActiveChanged += OnDeviceActiveChanged;
            _eventService.OnDeviceChanged += OnDeviceChanged;
        }

        private async void OnDeviceActiveChanged(object? sender, DeviceActiveChangedEventArgs e)
        {
            if (e.NewStatus)
            {
                await ConnectDeviceAsync(e.DeviceId, CancellationToken.None);
            }
            else
            {
                await DisconnectDeviceAsync(e.DeviceId, CancellationToken.None);
            }
        }

        private void OnDeviceChanged(object? sender, DeviceChangedEventArgs e)
        {
            switch (e.ChangeType)
            {
                case DataChangeType.Added:
                    // 当设备被添加时，加载并连接该设备
                    HandleDeviceAdded(e.Device);
                    break;
                case DataChangeType.Updated:
                    // 当设备被更新时，更新其配置
                    HandleDeviceUpdated(e.Device);
                    break;
                case DataChangeType.Deleted:
                    // 当设备被删除时，移除设备监控
                    HandleDeviceRemoved(e.Device.Id);
                    break;
            }
        }

        /// <summary>
        /// 处理设备添加事件
        /// </summary>
        private void HandleDeviceAdded(DeviceDto device)
        {
            if (device == null)
            {
                _logger.LogWarning("HandleDeviceAdded: 接收到空设备对象");
                return;
            }

            // 检查设备协议是否为OPC UA
            if (device.Protocol != Core.Enums.ProtocolType.OpcUa)
            {
                _logger.LogInformation("设备 {DeviceId} ({DeviceName}) 不是OPC UA协议，跳过加载", 
                    device.Id, device.Name);
                return;
            }

            try
            {
                _logger.LogInformation("处理设备添加事件: {DeviceId} ({DeviceName})", 
                    device.Id, device.Name);

                // 添加设备到监控列表
                AddDevice(device);

                // 如果设备是激活状态，则尝试连接
                if (device.IsActive)
                {
                    // 使用Task.Run来避免阻塞事件处理
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await ConnectDeviceAsync(device.Id, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "连接新添加的设备 {DeviceId} ({DeviceName}) 时发生错误", 
                                device.Id, device.Name);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理设备添加事件时发生错误: {DeviceId} ({DeviceName})", 
                    device?.Id, device?.Name);
            }
        }

        /// <summary>
        /// 处理设备更新事件
        /// </summary>
        private void HandleDeviceUpdated(DeviceDto device)
        {
            if (device == null)
            {
                _logger.LogWarning("HandleDeviceUpdated: 接收到空设备对象");
                return;
            }

            // 检查设备协议是否为OPC UA
            if (device.Protocol != Core.Enums.ProtocolType.OpcUa)
            {
                _logger.LogInformation("设备 {DeviceId} ({DeviceName}) 不是OPC UA协议，跳过更新", 
                    device.Id, device.Name);
                return;
            }

            try
            {
                _logger.LogInformation("处理设备更新事件: {DeviceId} ({DeviceName})", 
                    device.Id, device.Name);

                // 先移除旧设备配置
                if (_deviceContexts.TryGetValue(device.Id, out var oldContext))
                {
                    // 断开旧连接
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await DisconnectDeviceAsync(device.Id, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "断开旧设备 {DeviceId} ({DeviceName}) 连接时发生错误", 
                                device.Id, device.Name);
                        }
                    });
                }

                // 添加更新后的设备
                AddDevice(device);

                // 如果设备是激活状态，则尝试连接
                if (device.IsActive)
                {
                    // 使用Task.Run来避免阻塞事件处理
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await ConnectDeviceAsync(device.Id, CancellationToken.None);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "连接更新后的设备 {DeviceId} ({DeviceName}) 时发生错误", 
                                device.Id, device.Name);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理设备更新事件时发生错误: {DeviceId} ({DeviceName})", 
                    device?.Id, device?.Name);
            }
        }

        /// <summary>
        /// 处理设备删除事件
        /// </summary>
        private async void HandleDeviceRemoved(int deviceId)
        {
            try
            {
                _logger.LogInformation("处理设备删除事件: {DeviceId}", deviceId);

                // 从监控列表中移除设备
                await RemoveDeviceAsync(deviceId, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理设备删除事件时发生错误: {DeviceId}", deviceId);
            }
        }

        /// <summary>
        /// 初始化服务管理器
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("OPC UA服务管理器正在初始化...");
            // 初始化逻辑可以在需要时添加
            _logger.LogInformation("OPC UA服务管理器初始化完成");
        }

        /// <summary>
        /// 添加设备到监控列表
        /// </summary>
        public void AddDevice(DeviceDto device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (device.Protocol != ProtocolType.OpcUa)
            {
                _logger.LogWarning("设备 {DeviceId} 不是OPC UA协议，跳过添加", device.Id);
                return;
            }

            var context = new DeviceContext
                          {
                              Device = device,
                              OpcUaService = new OpcUaService(),
                              Variables = new ConcurrentDictionary<string, VariableDto>(),
                              IsConnected = false
                          };

            _deviceContexts.AddOrUpdate(device.Id, context, (key, oldValue) => context);
            _logger.LogInformation("已添加设备 {DeviceId} 到监控列表", device.Id);
        }

        /// <summary>
        /// 移除设备监控
        /// </summary>
        public async Task RemoveDeviceAsync(int deviceId, CancellationToken cancellationToken = default)
        {
            if (_deviceContexts.TryRemove(deviceId, out var context))
            {
                await DisconnectDeviceAsync(context, cancellationToken);
                _logger.LogInformation("已移除设备 {DeviceId} 的监控", deviceId);
            }
        }

        /// <summary>
        /// 更新设备变量
        /// </summary>
        public void UpdateVariables(int deviceId, List<VariableDto> variables)
        {
            if (_deviceContexts.TryGetValue(deviceId, out var context))
            {
                context.Variables.Clear();
                foreach (var variable in variables)
                {
                    context.Variables.AddOrUpdate(variable.OpcUaNodeId, variable, (key, oldValue) => variable);
                }

                _logger.LogInformation("已更新设备 {DeviceId} 的变量列表，共 {Count} 个变量", deviceId, variables.Count);
            }
        }

        /// <summary>
        /// 获取设备连接状态
        /// </summary>
        public bool IsDeviceConnected(int deviceId)
        {
            return _deviceContexts.TryGetValue(deviceId, out var context) && context.IsConnected;
        }

        /// <summary>
        /// 重新连接设备
        /// </summary>
        public async Task ReconnectDeviceAsync(int deviceId, CancellationToken cancellationToken = default)
        {
            if (_deviceContexts.TryGetValue(deviceId, out var context))
            {
                await DisconnectDeviceAsync(context, cancellationToken);
                await ConnectDeviceAsync(context, cancellationToken);
            }
        }

        /// <summary>
        /// 获取所有监控的设备ID
        /// </summary>
        public IEnumerable<int> GetMonitoredDeviceIds()
        {
            return _deviceContexts.Keys.ToList();
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        public async Task ConnectDeviceAsync(DeviceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null || string.IsNullOrEmpty(context.Device.OpcUaServerUrl))
                return;

            if (!context.Device.IsActive)
            {
                return;
            }

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("正在连接设备 {DeviceName} ({EndpointUrl})",
                                       context.Device.Name, context.Device.OpcUaServerUrl);

                var stopwatch = Stopwatch.StartNew();

                // 设置连接超时
                using var timeoutToken = new CancellationTokenSource(_options.ConnectionTimeoutMs);
                using var linkedToken
                    = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);

                await context.OpcUaService.ConnectAsync(context.Device.OpcUaServerUrl);

                stopwatch.Stop();
                _logger.LogInformation("设备 {DeviceName} 连接耗时 {ElapsedMs} ms",
                                       context.Device.Name, stopwatch.ElapsedMilliseconds);

                if (context.OpcUaService.IsConnected)
                {
                    context.IsConnected = true;
                    context.Device.IsRunning = true;
                    await SetupSubscriptionsAsync(context, cancellationToken);
                    _logger.LogInformation("设备 {DeviceName} 连接成功", context.Device.Name);
                }
                else
                {
                    context.IsConnected = false;
                    context.Device.IsRunning = false;
                    _logger.LogWarning("设备 {DeviceName} 连接失败", context.Device.Name);
                }
                
                _eventService.RaiseDeviceConnectChanged(
                                         this, new DeviceConnectChangedEventArgs(context.Device.Id, context.Device.Name,  context.IsConnected));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "连接设备 {DeviceName} 时发生错误: {ErrorMessage}",
                                 context.Device.Name, ex.Message);
                context.IsConnected = false;
                context.Device.IsRunning = false;
                _eventService.RaiseDeviceConnectChanged(
                    this, new DeviceConnectChangedEventArgs(context.Device.Id, context.Device.Name,  false));
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 断开设备连接
        /// </summary>
        private async Task DisconnectDeviceAsync(DeviceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null)
                return;

            try
            {
                _logger.LogInformation("正在断开设备 {DeviceName} 的连接", context.Device.Name);
                await context.OpcUaService.DisconnectAsync();
                context.IsConnected = false;
                context.Device.IsRunning = false;
                _eventService.RaiseDeviceConnectChanged(
                    this, new DeviceConnectChangedEventArgs(context.Device.Id, context.Device.Name,  false));
                _logger.LogInformation("设备 {DeviceName} 连接已断开", context.Device.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "断开设备 {DeviceName} 连接时发生错误: {ErrorMessage}",
                                 context.Device.Name, ex.Message);
            }
        }

        /// <summary>
        /// 设置订阅
        /// </summary>
        private async Task SetupSubscriptionsAsync(DeviceContext context, CancellationToken cancellationToken = default)
        {
            if (!context.IsConnected || !context.Variables.Any())
                return;

            try
            {
                _logger.LogInformation("正在为设备 {DeviceName} 设置订阅，变量数: {VariableCount}",
                                       context.Device.Name, context.Variables.Count);

                // 按PollingInterval对变量进行分组
                var variablesByPollingInterval = context.Variables.Values
                                                        .GroupBy(v => v.PollingInterval)
                                                        .ToDictionary(g => g.Key, g => g.ToList());

                // 为每个PollingInterval组设置单独的订阅
                foreach (var group in variablesByPollingInterval)
                {
                    int pollingInterval = group.Key;
                    var variables = group.Value;

                    _logger.LogInformation(
                        "为设备 {DeviceName} 设置PollingInterval {PollingInterval} 的订阅，变量数: {VariableCount}",
                        context.Device.Name, pollingInterval, variables.Count);


                    var opcUaNodes = variables
                                     .Select(v => new OpcUaNode { NodeId = v.OpcUaNodeId })
                                     .ToList();

                    context.OpcUaService.SubscribeToNode(opcUaNodes, HandleDataChanged,
                                                         pollingInterval, pollingInterval);
                }

                _logger.LogInformation("设备 {DeviceName} 订阅设置完成", context.Device.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "为设备 {DeviceName} 设置订阅时发生错误: {ErrorMessage}",
                                 context.Device.Name, ex.Message);
            }
        }

        /// <summary>
        /// 根据PollingInterval获取发布间隔（毫秒）
        /// </summary>
        private int GetPublishingIntervalFromPollLevel(int pollingInterval)
        {
            // 根据轮询间隔值映射到发布间隔
            return pollingInterval switch
                   {
                       100 => 100, // HundredMilliseconds -> 100ms发布间隔
                       500 => 500, // FiveHundredMilliseconds -> 500ms发布间隔
                       1000 => 1000, // OneSecond -> 1000ms发布间隔
                       5000 => 5000, // FiveSeconds -> 5000ms发布间隔
                       10000 => 10000, // TenSeconds -> 10000ms发布间隔
                       20000 => 20000, // TwentySeconds -> 20000ms发布间隔
                       30000 => 30000, // ThirtySeconds -> 30000ms发布间隔
                       60000 => 60000, // OneMinute -> 60000ms发布间隔
                       300000 => 300000, // FiveMinutes -> 300000ms发布间隔
                       600000 => 600000, // TenMinutes -> 600000ms发布间隔
                       1800000 => 1800000, // ThirtyMinutes -> 1800000ms发布间隔
                       3600000 => 3600000, // OneHour -> 3600000ms发布间隔
                       _ => _options.SubscriptionPublishingIntervalMs // 默认值
                   };
        }


        /// <summary>
        /// 处理数据变化
        /// </summary>
        private async void HandleDataChanged(OpcUaNode opcUaNode)
        {
            if (opcUaNode?.Value == null)
                return;

            try
            {
                // 查找对应的变量
                foreach (var context in _deviceContexts.Values)
                {
                    if (context.Variables.TryGetValue(opcUaNode.NodeId.ToString(), out var variable))
                    {
                        // 推送到数据处理队列
                        await _dataProcessingService.EnqueueAsync(new VariableContext(variable,opcUaNode.Value));
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理数据变化时发生错误: {ErrorMessage}", ex.Message);
            }
        }

        /// <summary>
        /// 连接指定设备
        /// </summary>
        public async Task ConnectDeviceAsync(int deviceId, CancellationToken cancellationToken = default)
        {
            if (_deviceContexts.TryGetValue(deviceId, out var context))
            {
                await ConnectDeviceAsync(context, cancellationToken);
            }
        }

        /// <summary>
        /// 批量连接设备
        /// </summary>
        public async Task ConnectDevicesAsync(IEnumerable<int> deviceIds, CancellationToken cancellationToken = default)
        {
            var connectTasks = new List<Task>();

            foreach (var deviceId in deviceIds)
            {
                if (_deviceContexts.TryGetValue(deviceId, out var context))
                {
                    connectTasks.Add(ConnectDeviceAsync(context, cancellationToken));
                }
            }

            await Task.WhenAll(connectTasks);
        }

        /// <summary>
        /// 断开指定设备连接
        /// </summary>
        public async Task DisconnectDeviceAsync(int deviceId, CancellationToken cancellationToken = default)
        {
            if (_deviceContexts.TryGetValue(deviceId, out var context))
            {
                await DisconnectDeviceAsync(context, cancellationToken);
            }
        }

        /// <summary>
        /// 批量断开设备连接
        /// </summary>
        public async Task DisconnectDevicesAsync(IEnumerable<int> deviceIds,
                                                 CancellationToken cancellationToken = default)
        {
            var disconnectTasks = new List<Task>();

            foreach (var deviceId in deviceIds)
            {
                disconnectTasks.Add(DisconnectDeviceAsync(deviceId, cancellationToken));
            }

            await Task.WhenAll(disconnectTasks);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _logger.LogInformation("正在释放OPC UA服务管理器资源...");

                // 断开所有设备连接
                var deviceIds = _deviceContexts.Keys.ToList();
                DisconnectDevicesAsync(deviceIds)
                    .Wait(TimeSpan.FromSeconds(10));

                // 释放其他资源
                _semaphore?.Dispose();

                _disposed = true;
                _logger.LogInformation("OPC UA服务管理器资源已释放");
            }
        }
    }

    /// <summary>
    /// 设备上下文
    /// </summary>
    public class DeviceContext
    {
        public DeviceDto Device { get; set; }
        public OpcUaService OpcUaService { get; set; }
        public ConcurrentDictionary<string, VariableDto> Variables { get; set; }
        public bool IsConnected { get; set; }
    }
}