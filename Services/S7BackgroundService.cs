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
    /// <summary>
    /// S7后台服务，继承自BackgroundService，用于在后台周期性地轮询S7 PLC设备数据。
    /// </summary>
    public class S7BackgroundService
    {
        // 数据服务实例，用于访问和操作应用程序数据，如设备配置。
        private readonly DataServices _dataServices;

        // 存储S7 PLC客户端实例的字典，键为设备ID，值为Plc对象。
        private readonly Dictionary<int, Plc> _s7PlcClients = new Dictionary<int, Plc>();

        // 轮询数据的线程。
        private Thread _pollingThread;

        // 用于取消轮询操作的CancellationTokenSource。
        private CancellationTokenSource _cancellationTokenSource;

        // 读取变量计数器。
        private int readCount = 0;

        // 跳过变量计数器（未到轮询时间）。
        private int TGCount = 0;

        // 定义不同轮询级别的间隔时间。
        private readonly Dictionary<PollLevelType, TimeSpan> _pollingIntervals = new Dictionary<PollLevelType, TimeSpan>
            {
                { PollLevelType.TenMilliseconds, TimeSpan.FromMilliseconds((int)PollLevelType.TenMilliseconds) },
                {
                    PollLevelType.HundredMilliseconds, TimeSpan.FromMilliseconds((int)PollLevelType.HundredMilliseconds)
                },
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

        // 存储S7设备列表。
        private List<Device>? _s7Devices;

        /// <summary>
        /// 构造函数，注入ILogger和DataServices。
        /// </summary>
        /// <param name="logger">日志记录器实例。</param>
        /// <param name="dataServices">数据服务实例。</param>
        public S7BackgroundService(DataServices dataServices)
        {
            _dataServices = dataServices;
            // 订阅设备列表变更事件，以便在设备配置更新时重新加载。
            _dataServices.OnDeviceListChanged += HandleDeviceListChanged;
        }

        /// <summary>
        /// 处理设备列表变更事件的回调方法。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="devices">更新后的设备列表。</param>
        private void HandleDeviceListChanged(object sender, List<Device> devices)
        {
            // 过滤出S7协议且激活的设备。
            _s7Devices = devices.Where(d => d.ProtocolType == ProtocolType.S7 && d.IsActive)
                                .ToList();
            // 当设备列表变化时，更新PLC客户端
            // 这里需要更复杂的逻辑来处理连接的关闭和新连接的建立
            // 简单起见，这里只做日志记录
            NlogHelper.Info("设备列表已更改。S7客户端可能需要重新初始化。");
        }

        /// <summary>
        /// 启动S7后台服务。
        /// </summary>
        public void StartService()
        {
            NlogHelper.Info("S7后台服务正在启动。");
            // 创建一个CancellationTokenSource，用于控制轮询线程的取消。
            _cancellationTokenSource = new CancellationTokenSource();

            // 创建并启动轮询线程。
            _pollingThread = new Thread(() => PollingLoop(_cancellationTokenSource.Token))
                             {
                                 IsBackground = true // 设置为后台线程，随主程序退出而退出
                             };
            _pollingThread.Start();
        }

        /// <summary>
        /// 轮询循环方法，在新线程中执行，周期性地读取S7设备数据。
        /// </summary>
        /// <param name="stoppingToken">用于取消轮询的CancellationToken。</param>
        private void PollingLoop(CancellationToken stoppingToken)
        {
            NlogHelper.Info("S7轮询线程已启动。");
            // 注册取消回调，当服务停止时记录日志。
            stoppingToken.Register(() => NlogHelper.Info("S7后台服务正在停止。"));

            // 初始加载S7设备列表。
            _s7Devices = _dataServices.Devices?.Where(d => d.ProtocolType == ProtocolType.S7 && d.IsActive)
                                      .ToList();

            // 轮询循环，直到收到取消请求。
            while (!stoppingToken.IsCancellationRequested)
            {
                // _logger.LogDebug("S7后台服务正在执行后台工作。");
                // _logger.LogDebug($"开始轮询变量,当前时间：{DateTime.Now}");
                readCount = 0;
                TGCount = 0;

                Stopwatch stopwatch = Stopwatch.StartNew(); // 启动计时器，测量轮询耗时
                PollS7Devices(stoppingToken); // 执行S7设备轮询
                stopwatch.Stop(); // 停止计时器
                // _logger.LogDebug($"结束轮询变量,当前时间：{DateTime.Now}");
                NlogHelper.Info($"读取变量数：{readCount}个，跳过变量数：{TGCount}",throttle:true);

                // 短暂休眠以防止CPU占用过高，并控制轮询频率。
                Thread.Sleep(100);
            }

            NlogHelper.Info("S7轮询线程已停止。");
        }


        /// <summary>
        /// 初始化或重新连接PLC客户端。
        /// </summary>
        /// <param name="device">S7设备。</param>
        /// <returns>连接成功的Plc客户端实例，如果连接失败则返回null。</returns>
        private Plc? InitializePlcClient(Device device)
        {
            // 检查字典中是否已存在该设备的PLC客户端。
            if (!_s7PlcClients.TryGetValue(device.Id, out var plcClient))
            {
                // 如果不存在，则创建新的Plc客户端。
                try
                {
                    plcClient = new Plc(device.CpuType, device.Ip, (short)device.Prot, device.Rack, device.Slot);
                    plcClient.Open(); // 尝试打开连接。
                    _s7PlcClients[device.Id] = plcClient; // 将新创建的客户端添加到字典。
                    NlogHelper.Info($"已连接到S7 PLC: {device.Name} ({device.Ip})");
                }
                catch (Exception ex)
                {
                    NotificationHelper.ShowError($"连接S7 PLC失败: {device.Name} ({device.Ip})", ex);
                    return null; // 连接失败，返回null。
                }
            }
            else if (!plcClient.IsConnected)
            {
                // 如果存在但未连接，则尝试重新连接。
                try
                {
                    plcClient.Open(); // 尝试重新打开连接。
                    NlogHelper.Info($"已重新连接到S7 PLC: {device.Name} ({device.Ip})");
                }
                catch (Exception ex)
                {
                    NotificationHelper.ShowError($"重新连接S7 PLC失败: {device.Name} ({device.Ip})", ex);
                    return null; // 重新连接失败，返回null。
                }
            }

            return plcClient; // 返回连接成功的Plc客户端。
        }

        /// <summary>
        /// 轮询S7设备数据。
        /// </summary>
        /// <param name="stoppingToken">取消令牌。</param>
        private void PollS7Devices(CancellationToken stoppingToken)
        {
            // 如果没有活跃的S7设备，则等待一段时间后重试。
            if (_s7Devices == null || !_s7Devices.Any())
            {
                NlogHelper.Info(
                    "未找到活跃的S7设备进行轮询。等待5秒后重试。",throttle:true);
                try
                {
                    // 使用CancellationToken来使等待可取消。
                    Task.Delay(TimeSpan.FromSeconds(5), stoppingToken)
                        .Wait(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // 如果在等待期间取消，则退出。
                    return;
                }

                return;
            }

            // 遍历所有S7设备。
            foreach (var device in _s7Devices)
            {
                if (stoppingToken.IsCancellationRequested) return; // 如果收到取消请求，则停止。

                // 尝试获取或初始化PLC客户端连接。
                var plcClient = InitializePlcClient(device);
                if (plcClient == null)
                {
                    continue; // 如果连接失败，则跳过当前设备。
                }


                // 读取设备变量。
                ReadDeviceVariables(plcClient, device, stoppingToken);
            }
        }

        /// <summary>
        /// 读取设备的S7变量并更新其值。
        /// </summary>
        /// <param name="plcClient">已连接的Plc客户端实例。</param>
        /// <param name="device">S7设备。</param>
        /// <param name="stoppingToken">取消令牌。</param>
        private void ReadDeviceVariables(Plc plcClient, Device device, CancellationToken stoppingToken)
        {
            // 过滤出当前设备和S7协议相关的变量。
            var s7Variables = device.VariableTables
                                    .Where(vt => vt.ProtocolType == ProtocolType.S7 && vt.IsActive)
                                    .SelectMany(vt => vt.DataVariables)
                                    .ToList();

            if (!s7Variables.Any())
            {
                NlogHelper.Info($"设备 {device.Name} 没有找到活跃的S7变量。");
                return;
            }

            try
            {
                // 遍历并读取每个S7变量。
                foreach (var variable in s7Variables)
                {
                    // Stopwatch stopwatch = Stopwatch.StartNew();
                    if (stoppingToken.IsCancellationRequested) return; // 如果取消令牌被请求，则停止读取。

                    // 获取变量的轮询间隔。
                    if (!_pollingIntervals.TryGetValue(variable.PollLevelType, out var interval))
                    {
                        NlogHelper.Info($"未知轮询级别 {variable.PollLevelType}，跳过变量 {variable.Name}。");
                        continue;
                    }

                    // 检查是否达到轮询时间。
                    if ((DateTime.Now - variable.LastPollTime) < interval)
                    {
                        TGCount++;
                        continue; // 未到轮询时间，跳过。
                    }

                    try
                    {
                        // 从PLC读取变量值。
                        var value = plcClient.Read(variable.S7Address);
                        if (value != null)
                        {
                            // 更新变量的原始数据值和显示值。
                            variable.DataValue = value.ToString();
                            variable.DisplayValue
                                = SiemensHelper.ConvertS7Value(value, variable.DataType, variable.Converstion);
                            variable.UpdateTime = DateTime.Now;

                            variable.LastPollTime = DateTime.Now; // 更新最后轮询时间。
                            readCount++;
                            // _logger.LogDebug($"线程ID：{Environment.CurrentManagedThreadId},已读取变量 {variable.Name}: {variable.DataValue}");
                        }
                    }
                    catch (Exception ex)
                    {
                        NotificationHelper.ShowError( $"从设备 {device.Name} 读取变量 {variable.Name} 失败。",ex);
                    }
                    // stopwatch.Stop();
                    //   NlogHelper.Info($"读取变量耗时:{stopwatch.ElapsedMilliseconds}ms ");
                }
            }
            catch (Exception ex)
            {
                NotificationHelper.ShowError($"设备 {device.Name} 批量读取过程中发生错误。",ex);
            }
        }

        /// <summary>
        /// 停止S7后台服务。
        /// </summary>
        public void StopService()
        {
            NlogHelper.Info("S7 Background Service is stopping.");

            // 发出信号，请求轮询线程停止。
            _cancellationTokenSource?.Cancel();
            // 等待轮询线程完成。
            _pollingThread?.Join();

            // 关闭所有活跃的PLC连接。
            foreach (var plcClient in _s7PlcClients.Values)
            {
                if (plcClient.IsConnected)
                {
                    plcClient.Close();
                    NlogHelper.Info($"Closed S7 PLC connection: {plcClient.IP}");
                }
            }

            _s7PlcClients.Clear(); // 清空PLC客户端字典。

            
        }
    }
}