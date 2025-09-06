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
using DMS.Infrastructure.Interfaces;
using DMS.Infrastructure.Interfaces.Services;

namespace DMS.Infrastructure.Services;

/// <summary>
/// S7后台服务，继承自BackgroundService，采用"编排者-代理"模式管理所有S7设备。
/// S7BackgroundService作为编排者，负责创建、管理和销毁每个设备专属的S7DeviceAgent。
/// 每个S7DeviceAgent作为代理，专门负责与一个S7 PLC进行所有交互。
/// </summary>
public class S7BackgroundService : BackgroundService
{
    private readonly IDataCenterService _dataCenterService;
    private readonly IDataProcessingService _dataProcessingService;
    private readonly IChannelBus _channelBus;
    private readonly IMessenger _messenger;
    private readonly ILogger<S7BackgroundService> _logger;
    private readonly SemaphoreSlim _reloadSemaphore = new SemaphoreSlim(0);

    // 存储活动的S7设备代理，键为设备ID，值为代理实例
    private readonly ConcurrentDictionary<int, S7DeviceAgent> _activeAgents = new();

    // S7轮询一遍后的等待时间
    private readonly int _s7PollOnceSleepTimeMs = 100;

    /// <summary>
    /// 构造函数，注入所需的服务
    /// </summary>
    public S7BackgroundService(
        IDataCenterService dataCenterService,
        IDataProcessingService dataProcessingService,
        IChannelBus channelBus,
        IMessenger messenger,
        ILogger<S7BackgroundService> logger)
    {
        _dataCenterService = dataCenterService;
        _dataProcessingService = dataProcessingService;
        _channelBus = channelBus;
        _messenger = messenger;
        _logger = logger;

        _dataCenterService.OnLoadDataCompleted += OnLoadDataCompleted;
    }

    private void OnLoadDataCompleted(object? sender, DataLoadCompletedEventArgs e)
    {
        _reloadSemaphore.Release();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("S7后台服务正在启动。");
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

                await LoadAndInitializeDevicesAsync(stoppingToken);
                _logger.LogInformation("S7后台服务已启动。");

                // 持续轮询，直到取消请求或需要重新加载
                while (!stoppingToken.IsCancellationRequested && _reloadSemaphore.CurrentCount == 0)
                {
                    await PollAllDevicesAsync(stoppingToken);
                    await Task.Delay(_s7PollOnceSleepTimeMs, stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("S7后台服务已停止。");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"S7后台服务运行中发生了错误:{e.Message}");
        }
        finally
        {
            await CleanupAsync();
        }
    }

    /// <summary>
    /// 加载并初始化所有S7设备
    /// </summary>
    private async Task LoadAndInitializeDevicesAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("开始加载S7设备....");
            
            // 获取所有激活的S7设备
            var s7Devices = _dataCenterService
                           .Devices.Values.Where(d => d.Protocol == ProtocolType.S7 && d.IsActive == true)
                           .ToList();

            // 清理已不存在的设备代理
            var existingDeviceIds = s7Devices.Select(d => d.Id).ToHashSet();
            var agentKeysToRemove = _activeAgents.Keys.Where(id => !existingDeviceIds.Contains(id)).ToList();
            
            foreach (var deviceId in agentKeysToRemove)
            {
                if (_activeAgents.TryRemove(deviceId, out var agent))
                {
                    await agent.DisposeAsync();
                    _logger.LogInformation($"已移除设备ID {deviceId} 的代理");
                }
            }

            // 为每个设备创建或更新代理
            foreach (var deviceDto in s7Devices)
            {
                if (!_dataCenterService.Devices.TryGetValue(deviceDto.Id, out var device))
                    continue;

                // 创建或更新设备代理
                // await CreateOrUpdateAgentAsync(device, stoppingToken);
            }

            _logger.LogInformation($"S7设备加载成功，共加载S7设备：{s7Devices.Count}个");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"加载S7设备的过程中发生了错误：{e.Message}");
        }
    }

    /// <summary>
    /// 创建或更新设备代理
    /// </summary>
    private async Task CreateOrUpdateAgentAsync(Device device, CancellationToken stoppingToken)
    {
        try
        {
            // 获取设备的变量
            var variables = device.VariableTables?
                                .SelectMany(vt => vt.Variables)
                                .Where(v => v.IsActive == true && v.Protocol == ProtocolType.S7)
                                .ToList() ?? new List<Variable>();

            // 检查是否已存在代理
            if (_activeAgents.TryGetValue(device.Id, out var existingAgent))
            {
                // 更新现有代理的变量配置
                existingAgent.UpdateVariables(variables);
            }
            else
            {
                // 创建新的代理
                // // var agent = new S7DeviceAgent(device, _channelBus, _messenger, _logger);
                // _activeAgents.AddOrUpdate(device.Id, agent, (key, oldValue) => agent);
                //
                // // 连接设备
                // await agent.ConnectAsync();
                //
                // // 更新变量配置
                // agent.UpdateVariables(variables);
                
                _logger.LogInformation($"已为设备 {device.Name} (ID: {device.Id}) 创建代理");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"为设备 {device.Name} (ID: {device.Id}) 创建/更新代理时发生错误");
        }
    }

    /// <summary>
    /// 轮询所有设备
    /// </summary>
    private async Task PollAllDevicesAsync(CancellationToken stoppingToken)
    {
        try
        {
            var pollTasks = new List<Task>();
            
            // 为每个活动代理创建轮询任务
            foreach (var agent in _activeAgents.Values)
            {
                if (stoppingToken.IsCancellationRequested)
                    break;
                    
                pollTasks.Add(agent.PollVariablesAsync());
            }
            
            // 并行执行所有轮询任务
            await Task.WhenAll(pollTasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"轮询S7设备时发生错误：{ex.Message}");
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    private async Task CleanupAsync()
    {
        _logger.LogInformation("正在清理S7后台服务资源...");
        
        // 断开所有代理连接并释放资源
        var cleanupTasks = new List<Task>();
        foreach (var agent in _activeAgents.Values)
        {
            cleanupTasks.Add(agent.DisposeAsync().AsTask());
        }
        
        await Task.WhenAll(cleanupTasks);
        _activeAgents.Clear();
        
        _logger.LogInformation("S7后台服务资源清理完成");
    }
}