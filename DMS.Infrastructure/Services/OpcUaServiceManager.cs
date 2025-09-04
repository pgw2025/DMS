using System.Collections.Concurrent;
using System.Diagnostics;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Infrastructure.Configuration;
using DMS.Infrastructure.Interfaces.Services;
using DMS.Infrastructure.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// OPC UA服务管理器，负责管理OPC UA连接、订阅和变量监控
    /// </summary>
    public class OpcUaServiceManager : IOpcUaServiceManager
    {
        private readonly ILogger<OpcUaServiceManager> _logger;
        private readonly IDataProcessingService _dataProcessingService;
        private readonly OpcUaServiceOptions _options;
        private readonly ConcurrentDictionary<int, DeviceContext> _deviceContexts;
        private readonly SemaphoreSlim _semaphore;
        private bool _disposed = false;

        public OpcUaServiceManager(
            ILogger<OpcUaServiceManager> logger,
            IDataProcessingService dataProcessingService,
            IOptions<OpcUaServiceOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dataProcessingService = dataProcessingService ?? throw new ArgumentNullException(nameof(dataProcessingService));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _deviceContexts = new ConcurrentDictionary<int, DeviceContext>();
            _semaphore = new SemaphoreSlim(_options.MaxConcurrentConnections, _options.MaxConcurrentConnections);
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

            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("正在连接设备 {DeviceName} ({EndpointUrl})", 
                    context.Device.Name, context.Device.OpcUaServerUrl);

                var stopwatch = Stopwatch.StartNew();
                
                // 设置连接超时
                using var timeoutToken = new CancellationTokenSource(_options.ConnectionTimeoutMs);
                using var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutToken.Token);
                
                await context.OpcUaService.ConnectAsync(context.Device.OpcUaServerUrl);
                
                stopwatch.Stop();
                _logger.LogInformation("设备 {DeviceName} 连接耗时 {ElapsedMs} ms", 
                    context.Device.Name, stopwatch.ElapsedMilliseconds);

                if (context.OpcUaService.IsConnected)
                {
                    context.IsConnected = true;
                    await SetupSubscriptionsAsync(context, cancellationToken);
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

                var opcUaNodes = context.Variables.Values
                    .Select(v => new OpcUaNode { NodeId = v.OpcUaNodeId })
                    .ToList();

                context.OpcUaService.SubscribeToNode(opcUaNodes, HandleDataChanged, 
                    _options.SubscriptionPublishingIntervalMs, _options.SubscriptionSamplingIntervalMs);

                _logger.LogInformation("设备 {DeviceName} 订阅设置完成", context.Device.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "为设备 {DeviceName} 设置订阅时发生错误: {ErrorMessage}", 
                    context.Device.Name, ex.Message);
            }
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
                        // 更新变量值
                        variable.DataValue = opcUaNode.Value.ToString();
                        variable.DisplayValue = opcUaNode.Value.ToString();
                        variable.UpdatedAt = DateTime.Now;

                        // 推送到数据处理队列
                        await _dataProcessingService.EnqueueAsync(variable);
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
                connectTasks.Add(ConnectDeviceAsync(deviceId, cancellationToken));
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
        public async Task DisconnectDevicesAsync(IEnumerable<int> deviceIds, CancellationToken cancellationToken = default)
        {
            var disconnectTasks = new List<Task>();
            
            foreach (var deviceId in deviceIds)
            {
                if (_deviceContexts.TryGetValue(deviceId, out var context))
                {
                    disconnectTasks.Add(DisconnectDeviceAsync(deviceId, cancellationToken));
                }
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
                DisconnectDevicesAsync(deviceIds).Wait(TimeSpan.FromSeconds(10));

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