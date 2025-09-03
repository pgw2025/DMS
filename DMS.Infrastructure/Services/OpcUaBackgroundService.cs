using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Core.Enums;
using DMS.Core.Models;
using Microsoft.Extensions.Hosting;
using Opc.Ua;
using Opc.Ua.Client;
using Microsoft.Extensions.Logging;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Infrastructure.Helper;
using DMS.Infrastructure.Models;

namespace DMS.Infrastructure.Services;

public class OpcUaBackgroundService : BackgroundService
{
    private readonly IDataCenterService _dataCenterService;

    // private readonly IDataProcessingService _dataProcessingService;
    private readonly ILogger<OpcUaBackgroundService> _logger;

    // 存储 OPC UA 设备，键为设备Id，值为会话对象。
    private readonly ConcurrentDictionary<int, DeviceDto> _opcUaDevices = new();

    // 存储 OPC UA 会话，键为终结点 URL，值为会话对象。
    private readonly ConcurrentDictionary<DeviceDto, OpcUaService> _opcUaServices;

    // 存储 OPC UA 订阅，键为终结点 URL，值为订阅对象。
    private readonly ConcurrentDictionary<string, Subscription> _opcUaSubscriptions;

    // 存储活动的 OPC UA 变量，键为变量的OpcNodeId
    private readonly ConcurrentDictionary<string, Variable> _opcUaPollVariablesByNodeId;

    // 储存所有要轮询更新的变量，键是Device.Id,值是这个设备所有要轮询的变量
    private readonly ConcurrentDictionary<int, List<Variable>> _opcUaPollVariablesByDeviceId;

    // 储存所有要订阅更新的变量，键是Device.Id,值是这个设备所有要轮询的变量
    private readonly ConcurrentDictionary<int, List<VariableDto>> _opcUaVariablesByDeviceId;

    private readonly SemaphoreSlim _reloadSemaphore = new SemaphoreSlim(0);

    // OPC UA 轮询间隔（毫秒）
    private readonly int _opcUaPollIntervalMs = 100;

    // OPC UA 订阅发布间隔（毫秒）
    private readonly int _opcUaSubscriptionPublishingIntervalMs = 1000;

    // OPC UA 订阅采样间隔（毫秒）
    private readonly int _opcUaSubscriptionSamplingIntervalMs = 1000;

    // 模拟 PollingIntervals，实际应用中可能从配置或数据库加载
    private static readonly Dictionary<PollLevelType, TimeSpan> PollingIntervals
        = new Dictionary<PollLevelType, TimeSpan>
          {
              { PollLevelType.TenMilliseconds, TimeSpan.FromMilliseconds((int)PollLevelType.TenMilliseconds) },
              { PollLevelType.HundredMilliseconds, TimeSpan.FromMilliseconds((int)PollLevelType.HundredMilliseconds) },
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

    public OpcUaBackgroundService(IDataCenterService dataCenterService,
                                  ILogger<OpcUaBackgroundService> logger)
    {
        _dataCenterService = dataCenterService;
        // _dataProcessingService = dataProcessingService;
        _logger = logger;
        _opcUaServices = new ConcurrentDictionary<DeviceDto, OpcUaService>();
        _opcUaSubscriptions = new ConcurrentDictionary<string, Subscription>();
        _opcUaPollVariablesByNodeId = new ConcurrentDictionary<string, Variable>();
        _opcUaPollVariablesByDeviceId = new ConcurrentDictionary<int, List<Variable>>();
        _opcUaVariablesByDeviceId = new ConcurrentDictionary<int, List<VariableDto>>();

        _dataCenterService.DataLoadCompleted += DataLoadCompleted;
    }

    private void DataLoadCompleted(object? sender, DataLoadCompletedEventArgs e)
    {
        _reloadSemaphore.Release();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OPC UA 后台服务正在启动。");
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
                    _logger.LogInformation("没有可用的OPC UA设备，等待设备列表更新...");
                    continue;
                }

                var isLoaded = LoadVariables();
                if (!isLoaded)
                {
                    _logger.LogInformation("加载变量过程中发生了错误，停止后面的操作。");
                    continue;
                }

                await ConnectOpcUaServiceAsync(stoppingToken);
                await SetupOpcUaSubscriptionAsync(stoppingToken);
                _logger.LogInformation("OPC UA 后台服务已启动。");

                // // 持续轮询，直到取消请求或需要重新加载
                // while (!stoppingToken.IsCancellationRequested && _reloadSemaphore.CurrentCount == 0)
                // {
                //     await PollOpcUaVariableOnceAsync(stoppingToken);
                //     await Task.Delay(_opcUaPollIntervalMs, stoppingToken);
                // }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("OPC UA 后台服务已停止。");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"OPC UA 后台服务运行中发生了错误:{e.Message}");
        }
        finally
        {
            await DisconnectAllOpcUaSessionsAsync();
        }
    }


    // private async void HandleDeviceIsActiveChanged(Device device, bool isActive)
    // {
    //     if (device.Protocol != ProtocolType.OpcUa)
    //         return;
    //
    //     _logger.LogInformation($"设备 {device.Name} (ID: {device.Id}) 的IsActive状态改变为 {isActive}。");
    //
    //     if (!isActive)
    //     {
    //         // 设备变为非活动状态，断开连接
    //         if (_opcUaServices.TryRemove(device.OpcUaServerUrl, out var session))
    //         {
    //             try
    //             {
    //                 if (_opcUaSubscriptions.TryRemove(device.OpcUaServerUrl, out var subscription))
    //                 {
    //                     // 删除订阅。
    //                     await subscription.DeleteAsync(true);
    //                     _logger.LogInformation($"已删除设备 {device.Name} ({device.OpcUaServerUrl}) 的订阅。");
    //                 }
    //
    //                 if (session.Connected)
    //                 {
    //                     await session.CloseAsync();
    //                     _logger.LogInformation($"已断开设备 {device.Name} ({device.OpcUaServerUrl}) 的连接。");
    //                 }
    //             }
    //             catch (Exception ex)
    //             {
    //                 _logger.LogError(ex, $"断开设备 {device.Name} ({device.OpcUaServerUrl}) 连接时发生错误：{ex.Message}");
    //             }
    //         }
    //     }
    //
    //     // 触发重新加载，让LoadVariables和ConnectOpcUaServiceAsync处理设备列表的更新
    //     _reloadSemaphore.Release();
    // }

    /// <summary>
    /// 从数据库加载所有活动的 OPC UA 变量，并进行相应的连接和订阅管理。
    /// </summary>
    private bool LoadVariables()
    {
        try
        {
            _opcUaDevices.Clear();
            _opcUaPollVariablesByDeviceId.Clear();
            _opcUaVariablesByDeviceId.Clear();
            _opcUaPollVariablesByNodeId.Clear();

            _logger.LogInformation("开始加载OPC UA变量....");
            var opcUaDevices = _dataCenterService
                               .Devices.Values.Where(d => d.Protocol == ProtocolType.OpcUa && d.IsActive == true)
                               .ToList();
            int totalVariableCount = 0;
            foreach (var opcUaDevice in opcUaDevices)
            {
                _opcUaDevices.AddOrUpdate(opcUaDevice.Id, opcUaDevice, (key, oldValue) => opcUaDevice);

                //查找设备中所有要订阅的变量
                var variableDtos = opcUaDevice.VariableTables?.SelectMany(vt => vt.Variables)
                                              .Where(vd => vd.IsActive == true &&
                                                           vd.Protocol == ProtocolType.OpcUa)
                                              .ToList();
                totalVariableCount += variableDtos.Count;
                _opcUaVariablesByDeviceId.AddOrUpdate(opcUaDevice.Id, variableDtos, (key, oldValue) => variableDtos);
            }

            _logger.LogInformation(
                $"OPC UA 变量加载成功，共加载OPC UA设备：{opcUaDevices.Count}个，变量数：{totalVariableCount}");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"加载OPC UA变量的过程中发生了错误：{e.Message}");
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
    private async Task ConnectSingleOpcUaDeviceAsync(DeviceDto device, CancellationToken stoppingToken = default)
    {
        // Check if already connected
        if (_opcUaServices.TryGetValue(device, out var existOpcUaService))
        {
            if (existOpcUaService.IsConnected)
            {
                _logger.LogInformation($"已连接到 OPC UA 服务器: {device.OpcUaServerUrl}");
                return;
            }
        }

        _logger.LogInformation($"开始连接OPC UA服务器: {device.Name} ({device.OpcUaServerUrl})");
        try
        {
            OpcUaService opcUaService = new OpcUaService();
            await opcUaService.ConnectAsync(device.OpcUaServerUrl);
            if (!opcUaService.IsConnected)
            {
                _logger.LogWarning($"创建OPC UA会话失败: {device.OpcUaServerUrl}");
                return; // 连接失败，直接返回
            }

            _opcUaServices.AddOrUpdate(device, opcUaService, (key, oldValue) => opcUaService);
            _logger.LogInformation($"已连接到OPC UA服务器: {device.Name} ({device.OpcUaServerUrl})");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"OPC UA服务连接 {device.Name} ({device.OpcUaServerUrl}) 过程中发生错误：{e.Message}");
        }
    }

    // private async Task PollOpcUaVariableOnceAsync(CancellationToken stoppingToken)
    // {
    //     try
    //     {
    //         var deviceIdsToPoll = _opcUaPollVariablesByDeviceId.Keys.ToList();
    //
    //         var pollingTasks = deviceIdsToPoll
    //                            .Select(deviceId => PollSingleDeviceVariablesAsync(deviceId, stoppingToken))
    //                            .ToList();
    //
    //         await Task.WhenAll(pollingTasks);
    //     }
    //     catch (OperationCanceledException)
    //     {
    //         _logger.LogInformation("OPC UA 后台服务轮询变量被取消。");
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, $"OPC UA 后台服务在轮询变量过程中发生错误：{ex.Message}");
    //     }
    // }

    // /// <summary>
    // /// 轮询单个设备的所有 OPC UA 变量。
    // /// </summary>
    // /// <param name="deviceId">设备的 ID。</param>
    // /// <param name="stoppingToken">取消令牌。</param>
    // private async Task PollSingleDeviceVariablesAsync(int deviceId, CancellationToken stoppingToken)
    // {
    //     if (stoppingToken.IsCancellationRequested) return;
    //
    //     if (!_opcUaDevices.TryGetValue(deviceId, out var device) || device.OpcUaServerUrl == null)
    //     {
    //         _logger.LogWarning($"OpcUa轮询变量时，在deviceDic中未找到ID为 {deviceId} 的设备，或其服务器地址为空，请检查！");
    //         return;
    //     }
    //
    //     if (!device.IsActive) return;
    //
    //     if (!_opcUaServices.TryGetValue(device.OpcUaServerUrl, out var session) || !session.Connected)
    //     {
    //         if (device.IsActive)
    //         {
    //             _logger.LogWarning($"用于 {device.OpcUaServerUrl} 的 OPC UA 会话未连接。正在尝试重新连接...");
    //             // await ConnectSingleOpcUaDeviceAsync(device, stoppingToken);
    //         }
    //
    //         return;
    //     }
    //
    //     if (!_opcUaPollVariablesByDeviceId.TryGetValue(deviceId, out var variableList) || variableList.Count == 0)
    //     {
    //         return;
    //     }
    //
    //     foreach (var variable in variableList)
    //     {
    //         if (stoppingToken.IsCancellationRequested) return;
    //
    //         if (!PollingIntervals.TryGetValue(variable.PollLevel, out var interval) ||
    //             (DateTime.Now - variable.UpdatedAt) < interval)
    //         {
    //             continue;
    //         }
    //
    //         await ReadAndProcessOpcUaVariableAsync(session, variable, stoppingToken);
    //     }
    // }

    /// <summary>
    /// 读取单个 OPC UA 变量并处理其数据。
    /// </summary>
    /// <param name="session">OPC UA 会话。</param>
    /// <param name="variable">要读取的变量。</param>
    /// <param name="stoppingToken">取消令牌。</param>
    private async Task ReadAndProcessOpcUaVariableAsync(Session session, Variable variable,
                                                        CancellationToken stoppingToken)
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
                _logger.LogWarning($"读取 OPC UA 变量 {variable.Name} ({variable.OpcUaNodeId}) 失败: {result.StatusCode}");
                return;
            }

            await UpdateAndEnqueueVariable(variable, result.Value);
        }
        catch (ServiceResultException ex) when (ex.StatusCode == StatusCodes.BadSessionIdInvalid)
        {
            _logger.LogError(ex, $"OPC UA会话ID无效，变量: {variable.Name} ({variable.OpcUaNodeId})。正在尝试重新连接...");
            // Assuming device can be retrieved from variable or passed as parameter if needed for ConnectSingleOpcUaDeviceAsync
            // For now, I'll just log and let the outer loop handle reconnection if the session is truly invalid for the device.
            // If a full device object is needed here, it would need to be passed down from PollSingleDeviceVariablesAsync.
            // For simplicity, I'll remove the direct reconnection attempt here and rely on the outer loop.
            // await ConnectSingleOpcUaDeviceAsync(variable.VariableTable.Device, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"轮询OPC UA变量 {variable.Name} ({variable.OpcUaNodeId}) 时发生未知错误: {ex.Message}");
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
            variable.UpdatedAt = DateTime.Now;
            // Console.WriteLine($"OpcUa后台服务轮询变量：{variable.Name},值：{variable.DataValue}");
            // 将更新后的数据推入处理队列。
            // await _dataProcessingService.EnqueueAsync(variable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"更新变量 {variable.Name} 并入队失败:{ex.Message}");
        }
    }

    /// <summary>
    /// 设置 OPC UA 订阅并添加监控项。
    /// </summary>
    /// <param name="stoppingToken">取消令牌。</param>
    private async Task SetupOpcUaSubscriptionAsync(CancellationToken stoppingToken)
    {
        foreach (var opcUaServiceKayValuePair in _opcUaServices)
        {
            var device = opcUaServiceKayValuePair.Key;
            var opcUaService = opcUaServiceKayValuePair.Value;

            if (_opcUaVariablesByDeviceId.TryGetValue(device.Id, out var opcUaVariables))
            {
                var variableGroup = opcUaVariables.GroupBy(variable => variable.PollLevel);
                foreach (var vGroup in variableGroup)
                {
                    var pollLevelType = vGroup.Key;
                    var opcUaNodes
                        = vGroup.Select(variableDto => new OpcUaNode() { NodeId = variableDto.OpcUaNodeId })
                                 .ToList();
                    opcUaService.SubscribeToNode(opcUaNodes,HandleDataChanged);
                }
            }
        }
    }

    private void HandleDataChanged(OpcUaNode opcUaNode)
    {
        
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
            _logger.LogInformation(
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
        if (_opcUaServices.IsEmpty)
            return;
        _logger.LogInformation("正在断开所有 OPC UA 会话...");
        var closeTasks = new List<Task>();

        foreach (var device in _opcUaServices.Keys.ToList())
        {
            closeTasks.Add(Task.Run(async () =>
            {
                _logger.LogInformation($"正在断开 OPC UA 会话: {device.Name}");
                if (_opcUaServices.TryRemove(device, out var opcUaService))
                {
                    await opcUaService.DisconnectAsync();
                    _logger.LogInformation($"已从 OPC UA 服务器断开连接: {device.Name}");
                }
            }));
        }

        await Task.WhenAll(closeTasks);
    }
}