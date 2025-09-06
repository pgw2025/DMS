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
    private readonly IDataProcessingService _dataProcessingService;

    // private readonly IDataProcessingService _dataProcessingService;
    private readonly ILogger<OpcUaBackgroundService> _logger;

    // 存储 OPC UA 设备，键为设备Id，值为会话对象。
    private readonly ConcurrentDictionary<int, DeviceDto> _opcUaDevices = new();

    // 存储 OPC UA 会话，键为终结点 URL，值为会话对象。
    private readonly ConcurrentDictionary<DeviceDto, OpcUaService> _opcUaServices;

    // 存储 OPC UA 订阅，键为终结点 URL，值为订阅对象。
    private readonly ConcurrentDictionary<string, Subscription> _opcUaSubscriptions;

    // 存储活动的 OPC UA 变量，键为变量的OpcNodeId
    private readonly ConcurrentDictionary<string, VariableDto> _opcUaVariables;

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
    private static readonly Dictionary<int, TimeSpan> PollingIntervals
        = new Dictionary<int, TimeSpan>
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

    public OpcUaBackgroundService(IDataCenterService dataCenterService,IDataProcessingService dataProcessingService, ILogger<OpcUaBackgroundService> logger)
    {
        _dataCenterService = dataCenterService;
        _dataProcessingService = dataProcessingService;
        _logger = logger;
        _opcUaServices = new ConcurrentDictionary<DeviceDto, OpcUaService>();
        _opcUaSubscriptions = new ConcurrentDictionary<string, Subscription>();
        _opcUaVariables = new ConcurrentDictionary<string, VariableDto>();
        _opcUaPollVariablesByDeviceId = new ConcurrentDictionary<int, List<Variable>>();
        _opcUaVariablesByDeviceId = new ConcurrentDictionary<int, List<VariableDto>>();

        _dataCenterService.OnLoadDataCompleted += OnLoadDataCompleted;
    }

    private void OnLoadDataCompleted(object? sender, DataLoadCompletedEventArgs e)
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
            _opcUaVariables.Clear();

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
                foreach (var variableDto in variableDtos)
                {
                    _opcUaVariables.TryAdd(variableDto.OpcUaNodeId,variableDto);
                }
                
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

    /// <summary>
    /// 更新变量数据，并将其推送到数据处理队列。
    /// </summary>
    /// <param name="variable">要更新的变量。</param>
    /// <param name="value">读取到的数据值。</param>
    private async Task UpdateAndEnqueueVariable(VariableDto variable, object value)
    {
        try
        {
            // 更新变量的原始数据值和显示值。
            variable.DataValue = value.ToString();
            variable.DisplayValue = value.ToString(); // 或者根据需要进行格式化
            variable.UpdatedAt = DateTime.Now;
            // Console.WriteLine($"OpcUa后台服务轮询变量：{variable.Name},值：{variable.DataValue}");
            // 将更新后的数据推入处理队列。
            await _dataProcessingService.EnqueueAsync(variable);
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
                var variableGroup = opcUaVariables.GroupBy(variable => variable.PollingInterval);
                foreach (var vGroup in variableGroup)
                {
                    var pollingInterval = vGroup.Key;
                    var opcUaNodes
                        = vGroup.Select(variableDto => new OpcUaNode() { NodeId = variableDto.OpcUaNodeId })
                                 .ToList();

                    PollingIntervals.TryGetValue(pollingInterval, out var interval);
                    opcUaService.SubscribeToNode(opcUaNodes,HandleDataChanged,10000,1000);
                }
            }
        }
    }

    private async void HandleDataChanged(OpcUaNode opcUaNode)
    {
        if (_opcUaVariables.TryGetValue(opcUaNode.NodeId.ToString(), out var variabelDto))
        {
            if (opcUaNode.Value == null)
            {
                 return;
            }
            await UpdateAndEnqueueVariable(variabelDto, opcUaNode.Value);
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