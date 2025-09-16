using System.Collections.Concurrent;
using System.Diagnostics;
using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Models;
using DMS.Core.Enums;
using DMS.Core.Events;
using DMS.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using DateTime = System.DateTime;

namespace DMS.Infrastructure.Services.S7;

/// <summary>
/// 优化的S7后台服务，继承自BackgroundService，用于在后台高效地轮询S7 PLC设备数据。
/// </summary>
public class OptimizedS7BackgroundService : BackgroundService
{
    private readonly IAppDataCenterService _appDataCenterService;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly IEventService _eventService;
    private readonly IDataProcessingService _dataProcessingService;
    private readonly IS7ServiceManager _s7ServiceManager;
    private readonly ILogger<OptimizedS7BackgroundService> _logger;
    private readonly SemaphoreSlim _reloadSemaphore = new SemaphoreSlim(0);

    // S7轮询一遍后的等待时间
    private readonly int _s7PollOnceSleepTimeMs = 50;

    // 存储每个设备的变量按轮询间隔分组
    private readonly ConcurrentDictionary<int, Dictionary<int, List<VariableDto>>> _variablesByPollingInterval = new();
    
   
    /// <summary>
    /// 构造函数，注入数据服务和数据处理服务。
    /// </summary>
    public OptimizedS7BackgroundService(
        IAppDataCenterService appDataCenterService,
        IAppDataStorageService appDataStorageService,
        IEventService eventService,
        IDataProcessingService dataProcessingService,
        IS7ServiceManager s7ServiceManager,
        ILogger<OptimizedS7BackgroundService> logger)
    {
        _appDataCenterService = appDataCenterService;
        _appDataStorageService = appDataStorageService;
        _eventService = eventService;
        _dataProcessingService = dataProcessingService;
        _s7ServiceManager = s7ServiceManager;
        _logger = logger;

        _appDataCenterService.DataLoaderService.OnLoadDataCompleted += OnLoadDataCompleted;
        
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

                if (_appDataStorageService.Devices.IsEmpty)
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
                    await PollS7VariablesAsync(stoppingToken);
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

            var s7Devices = _appDataStorageService
                            .Devices.Values.Where(d => d.Protocol == ProtocolType.S7 && d.IsActive == true)
                            .ToList();

            foreach (var s7Device in s7Devices)
            {
                _s7ServiceManager.AddDevice(s7Device);

                // 查找设备中所有要轮询的变量
                var variables = new List<VariableDto>();

                foreach (var variableTable in s7Device.VariableTables)
                {
                    if (variableTable.IsActive && variableTable.Protocol == ProtocolType.S7)
                    {
                        variables.AddRange(variableTable.Variables.Where(v => v.IsActive));
                    }
                }

                _s7ServiceManager.UpdateVariables(s7Device.Id, variables);
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
        var s7Devices = _appDataStorageService
                        .Devices.Values.Where(d => d.Protocol == ProtocolType.S7 && d.IsActive == true)
                        .ToList();

        var deviceIds = s7Devices.Select(d => d.Id)
                                 .ToList();
        await _s7ServiceManager.ConnectDevicesAsync(deviceIds, stoppingToken);

        _logger.LogInformation("已连接所有S7设备");
    }

    /// <summary>
    /// 按轮询间隔轮询S7变量
    /// </summary>
    private async Task PollS7VariablesAsync(CancellationToken stoppingToken)
    {
        try
        {
            var s7DeviceContexts = _s7ServiceManager.GetAllDeviceContexts();
            foreach (var context in s7DeviceContexts)
            {
                if (stoppingToken.IsCancellationRequested) break;
                
                // 收集该设备所有需要轮询的变量
                var variablesToPoll = context.Variables.Values.ToList();
                
                if (variablesToPoll.Any())
                {
                    await PollVariablesForDeviceAsync(context, variablesToPoll, stoppingToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"按轮询间隔轮询S7变量时发生错误：{ex.Message}");
        }
    }



    /// <summary>
    /// 轮询设备的变量
    /// </summary>
    private async Task PollVariablesForDeviceAsync(S7DeviceContext context, List<VariableDto> variables,
                                                   CancellationToken stoppingToken)
    {
        if (!_appDataStorageService.Devices.TryGetValue(context.Device.Id, out var device))
        {
            _logger.LogWarning($"轮询时没有找到设备ID：{context.Device.Id}");
            return;
        }

        var s7Service = context.S7Service;
        if (s7Service == null || !s7Service.IsConnected)
        {
            _logger.LogWarning($"轮询时设备 {device.Name} 没有连接或服务不可用");
            return;
        }

        try
        {
            var stopwatch = Stopwatch.StartNew();

            // 按地址分组变量以进行批量读取
            var addresses = variables.Where(v=>(DateTime.Now-v.UpdatedAt)>=TimeSpan.FromMilliseconds(v.PollingInterval)).Select(v => v.S7Address)
                                     .ToList();
            if (!addresses.Any())
            {
                return;
            }

            // 批量读取变量值
            var readResults = await s7Service.ReadVariablesAsync(addresses);

            stopwatch.Stop();
            _logger.LogDebug($"设备 {device.Name} 轮询 {variables.Count} 个变量耗时 {stopwatch.ElapsedMilliseconds} ms");

            // 更新变量值并推送到处理队列
            foreach (var variable in variables)
            {
                if (readResults.TryGetValue(variable.S7Address, out var value))
                {

                    // 将更新后的数据推入处理队列。
                    await _dataProcessingService.EnqueueAsync(new VariableContext(variable, value));
                }
                // else
                // {
                //     _logger.LogWarning($"未能从设备 {device.Name} 读取变量 {variable.S7Address} 的值");
                // }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"轮询设备 {device.Name} 的变量时发生错误：{ex.Message}");
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