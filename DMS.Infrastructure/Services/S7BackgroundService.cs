using System.Collections.Concurrent;
using DMS.Core.Enums;
using DMS.Core.Models;
using Microsoft.Extensions.Hosting;
using S7.Net;
using S7.Net.Types;
using DateTime = System.DateTime;
using Microsoft.Extensions.Logging;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using CpuType = DMS.Core.Enums.CpuType;

namespace DMS.Infrastructure.Services;

/// <summary>
/// S7后台服务，继承自BackgroundService，用于在后台周期性地轮询S7 PLC设备数据。
/// </summary>
public class S7BackgroundService : BackgroundService
{
    // 数据服务实例，用于访问和操作应用程序数据，如设备配置。
    private readonly IDeviceDataService _deviceDataService;

    // 数据处理服务实例，用于将读取到的数据推入处理队列。
    private readonly IDataProcessingService _dataProcessingService;

    // 存储 S7设备，键为设备Id，值为会话对象。
    private readonly ConcurrentDictionary<int, Device> _s7Devices;

    // 储存所有要轮询更新的变量，键是Device.Id,值是这个设备所有要轮询的变量
    private readonly ConcurrentDictionary<int, List<Variable>> _s7PollVariablesByDeviceId; // Key: Variable.Id

    // 存储S7 PLC客户端实例的字典，键为设备ID，值为Plc对象。
    private readonly ConcurrentDictionary<string, Plc> _s7PlcClientsByIp;

    // 储存所有变量的字典，方便通过id获取变量对象
    private readonly Dictionary<int, Variable> _s7VariablesById;

    //  S7轮询一次读取的变量数，不得大于15
    private readonly int _s7PollOnceReadMultipleVars = 9;

    //  S7轮询一遍后的等待时间
    private readonly int _s7PollOnceSleepTimeMs = 100;

    private readonly ILogger<S7BackgroundService> _logger;

    private readonly SemaphoreSlim _reloadSemaphore = new SemaphoreSlim(0);

    // 模拟 PollingIntervals，实际应用中可能从配置或数据库加载
    private static readonly Dictionary<PollLevelType, TimeSpan> PollingIntervals = new Dictionary<PollLevelType, TimeSpan>
    {
        { PollLevelType.TenMilliseconds, TimeSpan.FromMilliseconds((int)PollLevelType.TenMilliseconds) },
        { PollLevelType.HundredMilliseconds, TimeSpan.FromMilliseconds((int)PollLevelType.HundredMilliseconds) },
        { PollLevelType.FiveHundredMilliseconds, TimeSpan.FromMilliseconds((int)PollLevelType.FiveHundredMilliseconds) },
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
    /// 构造函数，注入数据服务和数据处理服务。
    /// </summary>
    /// <param name="deviceDataService">设备数据服务实例。</param>
    /// <param name="dataProcessingService">数据处理服务实例。</param>
    /// <param name="logger">日志记录器实例。</param>
    public S7BackgroundService(IDeviceDataService deviceDataService, IDataProcessingService dataProcessingService, ILogger<S7BackgroundService> logger)
    {
        _deviceDataService = deviceDataService;
        _dataProcessingService = dataProcessingService;
        _logger = logger;
        _s7Devices = new ConcurrentDictionary<int, Device>();
        _s7PollVariablesByDeviceId = new ConcurrentDictionary<int, List<Variable>>();
        _s7PlcClientsByIp = new ConcurrentDictionary<string, Plc>();
        _s7VariablesById = new();

        // 订阅设备列表变更事件，以便在设备配置更新时重新加载。
        _deviceDataService.OnDeviceListChanged += HandleDeviceListChanged;
        // 订阅单个设备IsActive状态变更事件
        _deviceDataService.OnDeviceIsActiveChanged += HandleDeviceIsActiveChanged;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("S7后台服务正在启动。");
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

                if (_deviceDataService.Devices == null || _deviceDataService.Devices.Count == 0)
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

                await ConnectS7Service(stoppingToken);
                _logger.LogInformation("S7后台服务开始轮询变量....");

                // 持续轮询，直到取消请求或需要重新加载
                while (!stoppingToken.IsCancellationRequested && _reloadSemaphore.CurrentCount == 0)
                {
                    await PollS7VariableOnce(stoppingToken);
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
            await DisconnectAllPlc();
        }
    }

    /// <summary>
    /// 处理设备列表变更事件的回调方法。
    /// </summary>
    /// <param name="devices">更新后的设备列表。</param>
    private void HandleDeviceListChanged(List<Device> devices)
    {
        _logger.LogInformation("设备列表已更改。S7客户端可能需要重新初始化。");
        _reloadSemaphore.Release(); // 触发ExecuteAsync中的全面重新加载
    }

    /// <summary>
    /// 处理单个设备IsActive状态变更事件。
    /// </summary>
    /// <param name="device">发生状态变化的设备。</param>
    /// <param name="isActive">设备新的IsActive状态。</param>
    private async void HandleDeviceIsActiveChanged(Device device, bool isActive)
    {
        if (device.Protocol != ProtocolType.S7)
            return;

        _logger.LogInformation($"设备 {device.Name} (ID: {device.Id}) 的IsActive状态改变为 {isActive}。");
        if (!isActive)
        {
            // 设备变为非活动状态，断开连接
            if (_s7PlcClientsByIp.TryRemove(device.IpAddress, out var plcClient))
            {
                try
                {
                    if (plcClient.IsConnected)
                    {
                        plcClient.Close();
                        _logger.LogInformation($"已断开设备 {device.Name} ({device.IpAddress}) 的连接。");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"断开设备 {device.Name} ({device.IpAddress}) 连接时发生错误：{ex.Message}");
                }
            }
        }

        // 触发重新加载，让LoadVariables和ConnectS7Service处理设备列表的更新
        _reloadSemaphore.Release();
    }

    private async Task PollS7VariableOnce(CancellationToken stoppingToken)
    {
        try
        {
            // 获取当前需要轮询的设备ID列表的快照
            var deviceIdsToPoll = _s7PollVariablesByDeviceId.Keys.ToList();

            // 为每个设备创建并发轮询任务
            var pollingTasks = deviceIdsToPoll.Select(async deviceId =>
            {
                if (!_s7Devices.TryGetValue(deviceId, out var device))
                {
                    _logger.LogWarning($"S7服务轮询时在deviceDic中没有找到Id为：{deviceId}的设备");
                    return; // 跳过此设备
                }

                if (!_s7PlcClientsByIp.TryGetValue(device.IpAddress, out var plcClient))
                {
                    _logger.LogWarning($"S7服务轮询时没有找到设备I：{deviceId}的初始化好的Plc客户端对象！");
                    return; // 跳过此设备
                }

                if (!plcClient.IsConnected)
                {
                    _logger.LogWarning($"S7服务轮询时设备：{device.Name},没有连接，跳过本次轮询。");
                    return; // 跳过此设备，等待ConnectS7Service重新连接
                }

                if (!_s7PollVariablesByDeviceId.TryGetValue(deviceId, out var variableList))
                {
                    _logger.LogWarning($"S7服务轮询时没有找到设备I：{deviceId},要轮询的变量列表！");
                    return; // 跳过此设备
                }

                // 轮询当前设备的所有变量
                var dataItemsToRead = new Dictionary<int, DataItem>(); // Key: Variable.Id, Value: DataItem
                var variablesToProcess = new List<Variable>(); // List of variables to process in this batch

                foreach (var variable in variableList)
                {
                    if (stoppingToken.IsCancellationRequested)
                    {
                        return; // 任务被取消，退出循环
                    }

                    // 获取变量的轮询间隔。
                    if (!PollingIntervals.TryGetValue(
                            variable.PollLevel, out var interval))
                    {
                        _logger.LogInformation($"未知轮询级别 {variable.PollLevel}，跳过变量 {variable.Name}。");
                        continue;
                    }

                    // 检查是否达到轮询时间。
                    if ((DateTime.Now - variable.UpdatedAt) < interval)
                        continue; // 未到轮询时间，跳过。

                    dataItemsToRead[variable.Id] = DataItem.FromAddress(variable.S7Address);
                    variablesToProcess.Add(variable);

                    // 达到批量读取数量或已是最后一个变量，执行批量读取
                    if (dataItemsToRead.Count >= _s7PollOnceReadMultipleVars || variable == variableList.Last())
                    {
                        try
                        {
                            // Perform the bulk read
                            await plcClient.ReadMultipleVarsAsync(dataItemsToRead.Values.ToList(), stoppingToken);

                            // Process the results
                            foreach (var varData in variablesToProcess)
                            {
                                if (dataItemsToRead.TryGetValue(varData.Id, out var dataItem))
                                {
                                    // Now dataItem has the updated value from the PLC
                                    await UpdateAndEnqueueVariable(varData, dataItem);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"从设备 {device.Name} 批量读取变量失败:{ex.Message}");
                        }
                        finally
                        {
                            dataItemsToRead.Clear();
                            variablesToProcess.Clear();
                        }
                    }
                }
            }).ToList();

            // 等待所有设备的轮询任务完成
            await Task.WhenAll(pollingTasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("S7后台服务轮询变量被取消。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"S7后台服务在轮询变量过程中发生错误：{ex.Message}");
        }
    }

    /// <summary>
    /// 更新变量数据，并将其推送到数据处理队列。
    /// </summary>
    /// <param name="variable">要更新的变量。</param>
    /// <param name="dataItem">包含读取到的数据项。</param>
    private async Task UpdateAndEnqueueVariable(Variable variable, DataItem dataItem)
    {
        try
        {
            // 更新变量的原始数据值和显示值。
            variable.DataValue = dataItem.Value.ToString();
            variable.DisplayValue = dataItem.Value.ToString();
            variable.UpdatedAt = DateTime.Now;
            // Console.WriteLine($"S7后台服务轮询变量：{variable.Name}，值：{variable.DataValue}");
            // 将更新后的数据推入处理队列。
            await _dataProcessingService.EnqueueAsync(variable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"更新变量 {variable.Name} 并入队失败:{ex.Message}");
        }
    }

    private async Task ConnectS7Service(CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        var connectTasks = new List<Task>();

        // 遍历_s7Devices中的所有设备，尝试连接
        foreach (var device in _s7Devices.Values.ToList())
        {
            connectTasks.Add(ConnectSingleDeviceAsync(device, stoppingToken));
        }

        await Task.WhenAll(connectTasks);
    }

    /// <summary>
    /// 连接单个S7 PLC设备。
    /// </summary>
    /// <param name="device">要连接的设备。</param>
    /// <param name="stoppingToken">取消令牌。</param>
    private async Task ConnectSingleDeviceAsync(Device device, CancellationToken stoppingToken = default)
    {
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        // Check if already connected
        if (_s7PlcClientsByIp.TryGetValue(device.IpAddress, out var existingPlc))
        {
            if (existingPlc.IsConnected)
            {
                _logger.LogInformation($"已连接到 S7 服务器: {device.IpAddress}:{device.Port}");
                return;
            }
            else
            {
                // Remove disconnected PLC from dictionary to attempt reconnection
                _s7PlcClientsByIp.TryRemove(device.IpAddress, out _);
            }
        }

        _logger.LogInformation($"开始连接S7 PLC: {device.Name} ({device.IpAddress})");
        try
        {
            var s7CpuType = ConvertCpuType(device.CpuType);
            var plcClient = new Plc(s7CpuType, device.IpAddress, (short)device.Port, device.Rack, device.Slot);
            await plcClient.OpenAsync(stoppingToken); // 尝试打开连接。

            _s7PlcClientsByIp.AddOrUpdate(device.IpAddress, plcClient, (key, oldValue) => plcClient);

            _logger.LogInformation($"已连接到S7 PLC: {device.Name} ({device.IpAddress})");
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"S7服务连接PLC {device.Name} ({device.IpAddress}) 过程中发生错误：{e.Message}");
        }
    }

    /// <summary>
    /// 将字符串形式的CPU类型转换为S7.Net.CpuType枚举。
    /// </summary>
    /// <param name="cpuTypeString">CPU类型的字符串表示。</param>
    /// <returns>对应的S7.Net.CpuType枚举值。</returns>
    /// <exception cref="ArgumentException">如果无法解析CPU类型字符串。</exception>
    private S7.Net.CpuType ConvertCpuType(CpuType cpuTypeString)
    {
        // if (Enum.TryParse(cpuTypeString, true, out S7.Net.CpuType cpuType))
        // {
        //     return cpuType;
        // }
        throw new ArgumentException($"无法解析CPU类型: {cpuTypeString}");
    }

    /// <summary>
    /// 加载变量
    /// </summary>
    private bool LoadVariables()
    {
        try
        {
            _s7Devices.Clear();
            _s7PollVariablesByDeviceId.Clear();
            _s7VariablesById.Clear(); // 确保在重新加载变量时清空此字典

            _logger.LogInformation("开始加载S7变量....");
            var s7Devices = _deviceDataService
                             .Devices.Where(d => d.IsActive == true && d.Protocol == ProtocolType.S7)
                             .ToList(); // 转换为列表，避免多次枚举

            int totalVariableCount = 0;
            foreach (var device in s7Devices)
            {
                // device.IsRuning = true;
                _s7Devices.AddOrUpdate(device.Id, device, (key, oldValue) => device);

                // 过滤出当前设备和S7协议相关的变量。
                var deviceS7Variables = device.VariableTables
                                        .Where(vt => vt.Protocol == ProtocolType.S7 && vt.IsActive && vt.Variables != null)
                                        .SelectMany(vt => vt.Variables)
                                        .Where(vd => vd.IsActive == true)
                                        .ToList(); // 转换为列表，避免多次枚举

                // 将变量存储到字典中，方便以后通过ID快速查找
                foreach (var s7Variable in deviceS7Variables)
                    _s7VariablesById[s7Variable.Id] = s7Variable;

                totalVariableCount += deviceS7Variables.Count; // 使用 Count 属性
                _s7PollVariablesByDeviceId.AddOrUpdate(device.Id, deviceS7Variables, (key, oldValue) => deviceS7Variables);
            }

            if (totalVariableCount == 0)
            {
                return false;
            }

            _logger.LogInformation($"S7变量加载成功，共加载S7设备：{s7Devices.Count}个，变量数：{totalVariableCount}");
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"S7后台服务加载变量时发生了错误:{e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 关闭所有PLC的连接
    /// </summary>
    private async Task DisconnectAllPlc()
    {
        if (_s7PlcClientsByIp.IsEmpty)
            return;

        // 创建一个任务列表，用于并发关闭所有PLC连接
        var closeTasks = new List<Task>();

        // 关闭所有活跃的PLC连接。
        foreach (var plcClient in _s7PlcClientsByIp.Values)
        {
            if (plcClient.IsConnected)
            {
                closeTasks.Add(Task.Run(() =>
                {
                    try
                    {
                        plcClient.Close();
                        _logger.LogInformation($"关闭S7连接: {plcClient.IP}");
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"S7后台服务关闭{plcClient.IP},后台连接时发生错误：{e.Message}");
                    }
                }));
            }
        }

        // 等待所有关闭任务完成
        await Task.WhenAll(closeTasks);
        _s7PlcClientsByIp.Clear(); // Clear the dictionary after all connections are attempted to be closed
    }
}