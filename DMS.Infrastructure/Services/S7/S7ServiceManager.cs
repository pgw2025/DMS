using System.Collections.Concurrent;
using System.Diagnostics;
using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Events;
using DMS.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.Record;

namespace DMS.Infrastructure.Services.S7
{
    /// <summary>
    /// S7服务管理器，负责管理S7连接和变量监控
    /// </summary>
    public class S7ServiceManager : IS7ServiceManager
    {
        private readonly ILogger<S7ServiceManager> _logger;
        private readonly IEventService _eventService;
        private readonly IDataProcessingService _dataProcessingService;
        private readonly IAppDataCenterService _appDataCenterService;
        private readonly IAppDataStorageService _appDataStorageService;
        private readonly IS7ServiceFactory _s7ServiceFactory;
        private readonly ConcurrentDictionary<int, S7DeviceContext> _deviceContexts;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;

        public S7ServiceManager(
            ILogger<S7ServiceManager> logger,
            IEventService eventService,
            IDataProcessingService dataProcessingService,
            IAppDataCenterService appDataCenterService,
            IAppDataStorageService appDataStorageService,
            IS7ServiceFactory s7ServiceFactory)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventService = eventService;
            _dataProcessingService
                = dataProcessingService ?? throw new ArgumentNullException(nameof(dataProcessingService));
            _appDataCenterService
                = appDataCenterService ?? throw new ArgumentNullException(nameof(appDataCenterService));
            _appDataStorageService = appDataStorageService;
            _s7ServiceFactory = s7ServiceFactory ?? throw new ArgumentNullException(nameof(s7ServiceFactory));
            _deviceContexts = new ConcurrentDictionary<int, S7DeviceContext>();
            _semaphore = new SemaphoreSlim(10, 10); // 默认最大并发连接数为10

            _eventService.OnVariableActiveChanged += OnVariableActiveChanged;
            _eventService.OnBatchImportVariables += OnBatchImportVariables;
            _eventService.OnVariableChanged += OnVariableChanged;
        }

        private void OnVariableActiveChanged(object? sender, VariablesActiveChangedEventArgs e)
        {
            if (_deviceContexts.TryGetValue(e.DeviceId, out var s7DeviceContext))
            {
                
                var variables = _appDataStorageService.Variables.Values.Where(v => e.VariableIds.Contains(v.Id))
                                                      .ToList();
                foreach (var variable in variables)
                {
                    if (e.NewStatus)
                    {
                        // 变量启用，从轮询列表中添加变量
                        s7DeviceContext.Variables.AddOrUpdate(variable.S7Address,variable, (key, oldValue) => variable);
                    }
                    else
                    {
                        // 变量停用，从轮询列表中移除变量
                        s7DeviceContext.Variables.Remove(variable.S7Address, out _);
                    }
                }
            }
        }

        /// <summary>
        /// 初始化服务管理器
        /// </summary>
        public async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("S7服务管理器正在初始化...");
            // 初始化逻辑可以在需要时添加
            _logger.LogInformation("S7服务管理器初始化完成");
        }

        /// <summary>
        /// 添加设备到监控列表
        /// </summary>
        public void AddDevice(DeviceDto device)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (device.Protocol != ProtocolType.S7)
            {
                _logger.LogWarning("设备 {DeviceId} 不是S7协议，跳过添加", device.Id);
                return;
            }

            var context = new S7DeviceContext
                          {
                              Device = device,
                              S7Service = _s7ServiceFactory.CreateService(),
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
                    context.Variables.AddOrUpdate(variable.S7Address, variable, (key, oldValue) => variable);
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
        /// 获取所有监控的设备ID
        /// </summary>
        public List<S7DeviceContext> GetAllDeviceContexts()
        {
            return _deviceContexts.Values.ToList();
        }

        /// <summary>
        /// 连接设备
        /// </summary>
        public async Task ConnectDeviceAsync(S7DeviceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null)
                return;

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("正在连接设备 {DeviceName} ({IpAddress}:{Port})",
                                       context.Device.Name, context.Device.IpAddress, context.Device.Port);

                var stopwatch = Stopwatch.StartNew();

                // 设置连接超时
                using var timeoutToken = new CancellationTokenSource(5000); // 5秒超时
                using var linkedToken
                    = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);

                var cpuType = ConvertCpuType(context.Device.CpuType);
                await context.S7Service.ConnectAsync(
                    context.Device.IpAddress,
                    context.Device.Port,
                    (short)context.Device.Rack,
                    (short)context.Device.Slot,
                    cpuType);

                stopwatch.Stop();
                _logger.LogInformation("设备 {DeviceName} 连接耗时 {ElapsedMs} ms",
                                       context.Device.Name, stopwatch.ElapsedMilliseconds);

                if (context.S7Service.IsConnected)
                {
                    context.IsConnected = true;


                    _logger.LogInformation("设备 {DeviceName} 连接成功", context.Device.Name);
                }
                else
                {
                    _logger.LogWarning("设备 {DeviceName} 连接失败", context.Device.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "连接设备 {DeviceName} 时发生错误: {ErrorMessage}",
                                 context.Device.Name, ex.Message);
                context.IsConnected = false;
            }
            finally
            {
                _eventService.RaiseDeviceStateChanged(
                    this,
                    new DeviceStateChangedEventArgs(context.Device.Id, context.Device.Name, context.IsConnected, Core.Enums.DeviceStateType.Connection));
                _semaphore.Release();
            }
        }

        /// <summary>
        /// 断开设备连接
        /// </summary>
        private async Task DisconnectDeviceAsync(S7DeviceContext context, CancellationToken cancellationToken = default)
        {
            if (context == null)
                return;

            try
            {
                _logger.LogInformation("正在断开设备 {DeviceName} 的连接", context.Device.Name);
                await context.S7Service.DisconnectAsync();
                context.IsConnected = false;

                _eventService.RaiseDeviceStateChanged(
                    this,
                    new DeviceStateChangedEventArgs(context.Device.Id, context.Device.Name, context.IsConnected, Core.Enums.DeviceStateType.Connection));
                _logger.LogInformation("设备 {DeviceName} 连接已断开", context.Device.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "断开设备 {DeviceName} 连接时发生错误: {ErrorMessage}",
                                 context.Device.Name, ex.Message);
            }
        }

        /// <summary>
        /// 将字符串形式的CPU类型转换为S7.Net.CpuType枚举
        /// </summary>
        private global::S7.Net.CpuType ConvertCpuType(CpuType cpuType)
        {
            return cpuType switch
                   {
                       CpuType.S7200 => global::S7.Net.CpuType.S7200,
                       CpuType.S7300 => global::S7.Net.CpuType.S7300,
                       CpuType.S7400 => global::S7.Net.CpuType.S7400,
                       CpuType.S71200 => global::S7.Net.CpuType.S71200,
                       CpuType.S71500 => global::S7.Net.CpuType.S71500,
                       _ => global::S7.Net.CpuType.S71200 // 默认值
                   };
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
                _logger.LogInformation("正在释放S7服务管理器资源...");

                // 断开所有设备连接
                var deviceIds = _deviceContexts.Keys.ToList();
                DisconnectDevicesAsync(deviceIds)
                    .Wait(TimeSpan.FromSeconds(10));

                // 释放其他资源
                _semaphore?.Dispose();

                _disposed = true;
                _logger.LogInformation("S7服务管理器资源已释放");
            }
        }

        /// <summary>
        /// 处理批量导入变量事件
        /// </summary>
        private void OnBatchImportVariables(object? sender, BatchImportVariablesEventArgs e)
        {
            if (e?.Variables == null || !e.Variables.Any())
            {
                _logger.LogWarning("OnBatchImportVariables: 接收到空变量列表");
                return;
            }

            try
            {
                _logger.LogInformation("处理批量导入变量事件，共 {Count} 个变量", e.Count);

                // 更新相关设备的变量表
                var deviceIds = e.Variables.Select(v => v.VariableTable.DeviceId).Distinct();
                foreach (var deviceId in deviceIds)
                {
                    // 获取设备的变量表信息
                    var variablesForDevice = e.Variables.Where(v => v.VariableTable.DeviceId == deviceId).ToList();
                    if (variablesForDevice.Any())
                    {
                        // 更新设备上下文中的变量
                        if (_deviceContexts.TryGetValue(deviceId, out var context))
                        {
                            // 将新导入的变量添加到设备上下文
                            foreach (var variable in variablesForDevice)
                            {
                                if (!context.Variables.ContainsKey(variable.S7Address))
                                {
                                    context.Variables.AddOrUpdate(variable.S7Address, variable, (key, oldValue) => variable);
                                }
                            }
                        }
                    }
                }
                
                _logger.LogInformation("批量导入变量事件处理完成，更新了 {DeviceCount} 个设备的变量信息", deviceIds.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理批量导入变量事件时发生错误");
            }
        }

        /// <summary>
        /// 处理变量变更事件
        /// </summary>
        private void OnVariableChanged(object? sender, VariableChangedEventArgs e)
        {
            try
            {
                _logger.LogDebug("处理变量变更事件: 变量ID={VariableId}, 变更类型={ChangeType}, 变更属性={PropertyType}", 
                    e.Variable.Id, e.ChangeType, e.PropertyType);

                // 根据变更类型和属性类型进行相应处理
                switch (e.ChangeType)
                {
                    case ActionChangeType.Updated:
                        // 如果变量的S7相关属性发生变化
                        switch (e.PropertyType)
                        {
                            case VariablePropertyType.S7Address:
                                // S7地址变化，需要更新设备上下文中的变量映射
                                if (_deviceContexts.TryGetValue(e.Variable.VariableTable.DeviceId, out var context))
                                {
                                    // 先移除旧地址的变量
                                    context.Variables.Remove(e.Variable.S7Address, out _);
                                    // 添加新地址的变量
                                    context.Variables.AddOrUpdate(e.Variable.S7Address, e.Variable, (key, oldValue) => e.Variable);
                                    _logger.LogInformation("已更新设备 {DeviceId} 中变量 {VariableId} 的S7地址映射", 
                                        e.Variable.VariableTable.DeviceId, e.Variable.Id);
                                }
                                break;
                                
                            case VariablePropertyType.IsActive:
                                // 变量激活状态变化，更新变量列表
                                if (_deviceContexts.TryGetValue(e.Variable.VariableTable.DeviceId, out var context2))
                                {
                                    if (e.Variable.IsActive)
                                    {
                                        // 添加变量到监控列表
                                        context2.Variables.AddOrUpdate(e.Variable.S7Address, e.Variable, (key, oldValue) => e.Variable);
                                    }
                                    else
                                    {
                                        // 从监控列表中移除变量
                                        context2.Variables.Remove(e.Variable.S7Address, out _);
                                    }
                                }
                                break;
                        }
                        break;
                        
                    case ActionChangeType.Deleted:
                        // 变量被删除时，从设备上下文的变量列表中移除
                        if (_deviceContexts.TryGetValue(e.Variable.VariableTable.DeviceId, out var context3))
                        {
                            context3.Variables.Remove(e.Variable.S7Address, out _);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理变量变更事件时发生错误: 变量ID={VariableId}, 变更类型={ChangeType}", 
                    e.Variable.Id, e.ChangeType);
            }
        }
    }

    /// <summary>
    /// S7设备上下文
    /// </summary>
    public class S7DeviceContext
    {
        public DeviceDto Device { get; set; }
        public IS7Service S7Service { get; set; }
        public ConcurrentDictionary<string, VariableDto> Variables { get; set; }
        public bool IsConnected { get; set; }
    }
}