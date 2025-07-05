using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using S7.Net;
using PMSWPF.Models;
using PMSWPF.Enums;
using PMSWPF.Helper;

namespace PMSWPF.Services
{
    public class S7BackgroundService : BackgroundService
    {
        private readonly ILogger<S7BackgroundService> _logger;
        private readonly DataServices _dataServices;
        private readonly Dictionary<int, Plc> _s7PlcClients = new Dictionary<int, Plc>();
        
        private readonly Dictionary<PollLevelType, TimeSpan> _pollingIntervals = new Dictionary<PollLevelType, TimeSpan>
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
        private List<Device>? _s7Devices;

        public S7BackgroundService(ILogger<S7BackgroundService> logger, DataServices dataServices)
        {
            _logger = logger;
            _dataServices = dataServices;
            _dataServices.OnDeviceListChanged += HandleDeviceListChanged;
        }

        private void HandleDeviceListChanged(List<Device> devices)
        {
            _s7Devices = devices.Where(d => d.ProtocolType == ProtocolType.S7 && d.IsActive)
                                .ToList();
            // 当设备列表变化时，更新PLC客户端
            // 这里需要更复杂的逻辑来处理连接的关闭和新连接的建立
            // 简单起见，这里只做日志记录
            _logger.LogInformation("设备列表已更改。S7客户端可能需要重新初始化。");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("S7后台服务正在启动。");

            stoppingToken.Register(() => _logger.LogInformation("S7后台服务正在停止。"));

            _s7Devices = _dataServices.Devices?.Where(d => d.ProtocolType == ProtocolType.S7 && d.IsActive)
                                      .ToList();

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("S7后台服务正在执行后台工作。");

                await PollS7Devices(stoppingToken);

            }

            _logger.LogInformation("S7后台服务已停止。");
        }

        /// <summary>
        /// 初始化或重新连接PLC客户端
        /// </summary>
        /// <param name="device">S7设备</param>
        /// <returns>连接成功的Plc客户端实例，如果连接失败则返回null</returns>
        private async Task<Plc?> InitializePlcClient(Device device)
        {
            // 检查字典中是否已存在该设备的PLC客户端
            if (!_s7PlcClients.TryGetValue(device.Id, out var plcClient))
            {
                // 如果不存在，则创建新的Plc客户端
                try
                {
                    plcClient = new Plc(device.CpuType, device.Ip, (short)device.Prot, device.Rack, device.Slot);
                    await plcClient.OpenAsync(); // 尝试打开连接
                    _s7PlcClients[device.Id] = plcClient; // 将新创建的客户端添加到字典
                    _logger.LogInformation($"已连接到S7 PLC: {device.Name} ({device.Ip})");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"连接S7 PLC失败: {device.Name} ({device.Ip})");
                    return null; // 连接失败，返回null
                }
            }
            else if (!plcClient.IsConnected)
            {
                // 如果存在但未连接，则尝试重新连接
                try
                {
                    await plcClient.OpenAsync(); // 尝试重新打开连接
                    _logger.LogInformation($"已重新连接到S7 PLC: {device.Name} ({device.Ip})");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"重新连接S7 PLC失败: {device.Name} ({device.Ip})");
                    return null; // 重新连接失败，返回null
                }
            }
            return plcClient; // 返回连接成功的Plc客户端
        }

        /// <summary>
        /// 轮询S7设备数据
        /// </summary>
        /// <param name="stoppingToken">取消令牌</param>
        private async Task PollS7Devices(CancellationToken stoppingToken)
        {
            if (_s7Devices == null || !_s7Devices.Any())
            {
                _logger.LogDebug("未找到活跃的S7设备进行轮询。等待5秒后重试。");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                return;
            }

            foreach (var device in _s7Devices)
            {
                if (stoppingToken.IsCancellationRequested) return;

                // 尝试获取或初始化PLC客户端连接
                var plcClient = await InitializePlcClient(device);
                if (plcClient == null)
                {
                    continue; // 如果连接失败，则跳过当前设备
                }

                
                // 读取设备变量
                await ReadDeviceVariables(plcClient, device, stoppingToken);
                
            }
        }

        /// <summary>
        /// 读取设备的S7变量并更新其值。
        /// </summary>
        /// <param name="plcClient">已连接的Plc客户端实例。</param>
        /// <param name="device">S7设备。</param>
        /// <param name="stoppingToken">取消令牌。</param>
        private async Task ReadDeviceVariables(Plc plcClient, Device device, CancellationToken stoppingToken)
        {
            // 过滤出当前设备和S7协议相关的变量
            var s7Variables = device.VariableTables
                                    .Where(vt => vt.ProtocolType == ProtocolType.S7 && vt.IsActive)
                                    .SelectMany(vt => vt.DataVariables)
                                    .ToList();

            if (!s7Variables.Any())
            {
                _logger.LogDebug($"设备 {device.Name} 没有找到活跃的S7变量。");
                return;
            }

            try
            {
                Stopwatch sw = Stopwatch.StartNew();
                // 遍历并读取每个S7变量
                foreach (var variable in s7Variables)
                {
                    if (stoppingToken.IsCancellationRequested) return; // 如果取消令牌被请求，则停止读取

                    // 获取变量的轮询间隔
                    if (!_pollingIntervals.TryGetValue(variable.PollLevelType, out var interval))
                    {
                        _logger.LogWarning($"未知轮询级别 {variable.PollLevelType}，跳过变量 {variable.Name}。");
                        continue;
                    }

                    // 检查是否达到轮询时间
                    if ((DateTime.Now - variable.LastPollTime) < interval)
                    {
                        continue; // 未到轮询时间，跳过
                    }

                    try
                    {
                        // 从PLC读取变量值
                        var value = await plcClient.ReadAsync(variable.S7Address);
                        if (value != null)
                        {
                            // 更新变量的原始数据值和显示值
                            variable.DataValue = value.ToString();
                            variable.DisplayValue = SiemensHelper.ConvertS7Value(value, variable.DataType, variable.Converstion);
                            variable.LastPollTime = DateTime.Now; // 更新最后轮询时间
                            _logger.LogDebug($"已读取变量 {variable.Name}: {variable.DataValue}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"从设备 {device.Name} 读取变量 {variable.Name} 失败。");
                    }
                }
                
                sw.Stop();
                _logger.LogInformation($"从: {device.Name} ({device.Ip})读取变量总耗时：{sw.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"设备 {device.Name} 批量读取过程中发生错误。");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("S7 Background Service is stopping.");

            // Close all active PLC connections
            foreach (var plcClient in _s7PlcClients.Values)
            {
                if (plcClient.IsConnected)
                {
                    plcClient.Close();
                    _logger.LogInformation($"Closed S7 PLC connection: {plcClient.IP}");
                }
            }

            _s7PlcClients.Clear();

            await base.StopAsync(stoppingToken);
        }
    }
}