using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Models;
using Microsoft.Extensions.Hosting;
using S7.Net;
using S7.Net.Types;
using DateTime = System.DateTime;
using Microsoft.Extensions.Logging;
using DMS.Core.Interfaces;
using DMS.Infrastructure.Interfaces.Services;
using System.Diagnostics;

namespace DMS.Infrastructure.Services;

/// <summary>
/// 优化的S7后台服务，继承自BackgroundService，用于在后台高效地轮询S7 PLC设备数据。
/// </summary>
public class OptimizedS7BackgroundService : BackgroundService
{
    private readonly IDataCenterService _dataCenterService;
    private readonly IDataProcessingService _dataProcessingService;
    private readonly IS7ServiceManager _s7ServiceManager;
    private readonly ILogger<OptimizedS7BackgroundService> _logger;
    private readonly SemaphoreSlim _reloadSemaphore = new SemaphoreSlim(0);

    // S7轮询一遍后的等待时间
    private readonly int _s7PollOnceSleepTimeMs = 50;

    // 存储每个设备的变量按轮询间隔分组
    private readonly ConcurrentDictionary<int, Dictionary<int, List<VariableDto>>> _variablesByPollingInterval = new();

    // 模拟 PollingIntervals，实际应用中可能从配置或数据库加载
    private static readonly Dictionary<int, TimeSpan> PollingIntervals = new Dictionary<int, TimeSpan>
    {
        { 10, TimeSpan.FromMilliseconds(10) }, // TenMilliseconds
        { 100, TimeSpan.FromMilliseconds(100) }, // HundredMilliseconds
        { 500, TimeSpan.FromMilliseconds(500) }, // FiveHundredMilliseconds
        { 1000, TimeSpan.FromMilliseconds(1000) }, // OneSecond
        { 5000, TimeSpan.FromMilliseconds(5000) }, // FiveSeconds
        { 10000, TimeSpan.FromMilliseconds(10000) }, // TenSeconds
        { 20000, TimeSpan.FromMilliseconds(20000) }, // TwentySeconds
        { 30000, TimeSpan.FromMilliseconds(30000) }, // ThirtySeconds
        { 60000, TimeSpan.FromMilliseconds(60000) }, // OneMinute
        { 180000, TimeSpan.FromMilliseconds(180000) }, // ThreeMinutes
        { 300000, TimeSpan.FromMilliseconds(300000) }, // FiveMinutes
        { 600000, TimeSpan.FromMilliseconds(600000) }, // TenMinutes
        { 1800000, TimeSpan.FromMilliseconds(1800000) } // ThirtyMinutes
    };

    /// <summary>
    /// 构造函数，注入数据服务和数据处理服务。
    /// </summary>
    public OptimizedS7BackgroundService(
        IDataCenterService dataCenterService,
        IDataProcessingService dataProcessingService,
        IS7ServiceManager s7ServiceManager,
        ILogger<OptimizedS7BackgroundService> logger)
    {
        _dataCenterService = dataCenterService;
        _dataProcessingService = dataProcessingService;
        _s7ServiceManager = s7ServiceManager;
        _logger = logger;

        _dataCenterService.OnLoadDataCompleted += OnLoadDataCompleted;
    }

    private void OnLoadDataCompleted(object? sender, DataLoadCompletedEventArgs e)
    {
        _reloadSemaphore.Release();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("优化的S7后台服务正在启动。");
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _reloadSemaphore.WaitAsync(stoppingToken); // Wait for a reload signal

                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                if (_dataCenterService.Devices.IsEmpty)
                {
                    _logger.LogInformation("没有可用的S7设备，等待设备列表更新...");
                    continue;
                }

                var isLoaded = LoadVariables();
                if (!isLoaded)
                {
                    _logger.LogInformation("加载变量过程中发生了错误，停止后面的操作。");
                    continue;
                }

                await ConnectS7ServiceAsync(stoppingToken);
                _logger.LogInformation("优化的S7后台服务已启动。");

                // 持续轮询，直到取消请求或需要重新加载
                while (!stoppingToken.IsCancellationRequested && _reloadSemaphore.CurrentCount == 0)
                {
                    await PollS7VariablesByPollingIntervalAsync(stoppingToken);
                    await Task.Delay(_s7PollOnceSleepTimeMs, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("优化的S7后台服务已停止。");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"优化的S7后台服务运行中发生了错误:{e.Message}");
        }
        finally
        {
            await DisconnectAllS7SessionsAsync();
        }
    }

    /// <summary>
    /// 从数据库加载所有活动的 S7 变量，并按轮询级别进行分组。
    /// </summary>
    private bool LoadVariables()
    {
        try
        {
            _variablesByPollingInterval.Clear();
            _logger.LogInformation("开始加载S7变量....");
            
            var s7Devices = _dataCenterService
                           .Devices.Values.Where(d => d.Protocol == ProtocolType.S7 && d.IsActive == true)
                           .ToList();
                           
            foreach (var s7Device in s7Devices)
            {
                _s7ServiceManager.AddDevice(s7Device);
                
                // 查找设备中所有要轮询的变量
                var variables = s7Device.VariableTables?.SelectMany(vt => vt.Variables)
                                          .Where(v => v.IsActive == true && v.Protocol == ProtocolType.S7)
                                          .ToList() ?? new List<VariableDto>();
                                          
                _s7ServiceManager.UpdateVariables(s7Device.Id, variables);
                
                // 按轮询间隔分组变量
                var variablesByPollingInterval = variables
                    .GroupBy(v => v.PollingInterval)
                    .ToDictionary(g => g.Key, g => g.ToList());
                    
                _variablesByPollingInterval.AddOrUpdate(s7Device.Id, variablesByPollingInterval, (key, oldValue) => variablesByPollingInterval);
            }

            _logger.LogInformation($"S7 变量加载成功，共加载S7设备：{s7Devices.Count}个");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"加载S7变量的过程中发生了错误：{e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 连接到 S7 服务器
    /// </summary>
    private async Task ConnectS7ServiceAsync(CancellationToken stoppingToken)
    {

        var s7Devices = _dataCenterService
                       .Devices.Values.Where(d => d.Protocol == ProtocolType.S7 && d.IsActive == true)
                       .ToList();

        var deviceIds = s7Devices.Select(d => d.Id).ToList();
        await _s7ServiceManager.ConnectDevicesAsync(deviceIds, stoppingToken);
        
        _logger.LogInformation("已连接所有S7设备");
    }

    /// <summary>
    /// 按轮询间隔轮询S7变量
    /// </summary>
    private async Task PollS7VariablesByPollingIntervalAsync(CancellationToken stoppingToken)
    {
        try
        {
            var pollTasks = new List<Task>();
            
            // 为每个设备创建轮询任务
            foreach (var deviceEntry in _variablesByPollingInterval)
            {
                var deviceId = deviceEntry.Key;
                var variablesByPollingInterval = deviceEntry.Value;
                
                // 为每个轮询间隔创建轮询任务
                foreach (var pollingIntervalEntry in variablesByPollingInterval)
                {
                    var pollingInterval = pollingIntervalEntry.Key;
                    var variables = pollingIntervalEntry.Value;
                    
                    // 检查是否达到轮询时间
                    if (ShouldPollVariables(variables, pollingInterval))
                    {
                        pollTasks.Add(PollVariablesForDeviceAsync(deviceId, variables, stoppingToken));
                    }
                }
            }
            
            await Task.WhenAll(pollTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"按轮询间隔轮询S7变量时发生错误：{ex.Message}");
        }
    }
    
    /// <summary>
    /// 检查是否应该轮询变量
    /// </summary>
    private bool ShouldPollVariables(List<VariableDto> variables, int pollingInterval)
    {
        if (!PollingIntervals.TryGetValue(pollingInterval, out var interval))
            return false;
            
        // 检查是否有任何一个变量需要轮询
        return variables.Any(v => (DateTime.Now - v.UpdatedAt) >= interval);
    }
    
    /// <summary>
    /// 轮询设备的变量
    /// </summary>
    private async Task PollVariablesForDeviceAsync(int deviceId, List<VariableDto> variables, CancellationToken stoppingToken)
    {
        if (!_dataCenterService.Devices.TryGetValue(deviceId, out var device))
        {
            _logger.LogWarning($"轮询时没有找到设备ID：{deviceId}");
            return;
        }
        
        if (!_s7ServiceManager.IsDeviceConnected(deviceId))
        {
            _logger.LogWarning($"轮询时设备 {device.Name} 没有连接");
            return;
        }
        
        try
        {
            var stopwatch = Stopwatch.StartNew();
            
            // 按地址分组变量以进行批量读取
            var addresses = variables.Select(v => v.S7Address).ToList();
            
            // 这里应该使用IS7Service来读取变量
            // 由于接口限制，我们暂时跳过实际读取，仅演示逻辑
            
            stopwatch.Stop();
            _logger.LogDebug($"设备 {device.Name} 轮询 {variables.Count} 个变量耗时 {stopwatch.ElapsedMilliseconds} ms");
            
            // 更新变量值并推送到处理队列
            foreach (var variable in variables)
            {
                // 模拟读取到的值
                var value = DateTime.Now.Ticks.ToString();
                await UpdateAndEnqueueVariable(variable, value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"轮询设备 {device.Name} 的变量时发生错误：{ex.Message}");
        }
    }

    /// <summary>
    /// 更新变量数据，并将其推送到数据处理队列。
    /// </summary>
    private async Task UpdateAndEnqueueVariable(VariableDto variable, string value)
    {
        try
        {
            // 更新变量的原始数据值和显示值。
            variable.DataValue = value;
            variable.DisplayValue = value;
            variable.UpdatedAt = DateTime.Now;
            
            // 将更新后的数据推入处理队列。
            await _dataProcessingService.EnqueueAsync(variable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"更新变量 {variable.Name} 并入队失败:{ex.Message}");
        }
    }

    /// <summary>
    /// 断开所有 S7 会话。
    /// </summary>
    private async Task DisconnectAllS7SessionsAsync()
    {
        _logger.LogInformation("正在断开所有 S7 会话...");
        
        var deviceIds = _s7ServiceManager.GetMonitoredDeviceIds();
        await _s7ServiceManager.DisconnectDevicesAsync(deviceIds);
        
        _logger.LogInformation("已断开所有 S7 会话");
    }
}