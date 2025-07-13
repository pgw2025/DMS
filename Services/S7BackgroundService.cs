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
using S7.Net.Types;
using SqlSugar;
using DateTime = System.DateTime;

namespace PMSWPF.Services
{
    /// <summary>
    /// S7后台服务，继承自BackgroundService，用于在后台周期性地轮询S7 PLC设备数据。
    /// </summary>
    public class S7BackgroundService
    {
        // 数据服务实例，用于访问和操作应用程序数据，如设备配置。
        private readonly DataServices _dataServices;

        // 存储 S7设备，键为设备Id，值为会话对象。
        private readonly Dictionary<int, Device> _deviceDic;

        // 储存所有要轮询更新的变量，键是Device.Id,值是这个设备所有要轮询的变量
        private readonly Dictionary<int, List<VariableData>> _pollVariableDic; // Key: VariableData.Id

        // 存储S7 PLC客户端实例的字典，键为设备ID，值为Plc对象。
        private readonly Dictionary<string, Plc> _s7PlcClientDic;

        // 储存所有变量的字典，方便通过id获取变量对象
        private readonly Dictionary<int, VariableData> _variableDic;
        //  S7轮询一次读取的变量数，不得大于15
        private readonly int S7PollOnceReadMultipleVars = 9;
        //  S7轮询一遍后的等待时间
        private readonly int S7PollOnceSleepTimeMs = 100;

        // 轮询数据的线程。
        private Thread _pollingThread;

        private ManualResetEvent _reloadEvent = new ManualResetEvent(false);
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);


        // 存储S7设备列表。
        private Thread _serviceMainThread;

        /// <summary>
        /// 构造函数，注入ILogger和DataServices。
        /// </summary>
        /// <param name="logger">日志记录器实例。</param>
        /// <param name="dataServices">数据服务实例。</param>
        public S7BackgroundService(DataServices dataServices)
        {
            _dataServices = dataServices;
            _deviceDic = new();
            _pollVariableDic = new();
            _s7PlcClientDic = new();
            _variableDic = new();
            // 订阅设备列表变更事件，以便在设备配置更新时重新加载。
            _dataServices.OnDeviceListChanged += HandleDeviceListChanged;
        }

        /// <summary>
        /// 处理设备列表变更事件的回调方法。
        /// </summary>
        /// <param name="sender">事件发送者。</param>
        /// <param name="devices">更新后的设备列表。</param>
        private void HandleDeviceListChanged(List<Device> devices)
        {
            NlogHelper.Info("设备列表已更改。S7客户端可能需要重新初始化。");
            _reloadEvent.Set();
        }

        /// <summary>
        /// 启动S7后台服务。
        /// </summary>
        public void StartService()
        {
            NlogHelper.Info("S7后台服务正在启动。");
            _reloadEvent.Set();
            _serviceMainThread = new Thread(Execute);
            _serviceMainThread.IsBackground = true;
            _serviceMainThread.Name = "S7ServiceMainThread";
            _serviceMainThread.Start();
        }

        private void Execute()
        {
            try
            {
                while (!_stopEvent.WaitOne(0))
                {
                    _reloadEvent.WaitOne();
                    if (_dataServices.Devices == null || _dataServices.Devices.Count == 0)
                    {
                        _reloadEvent.Reset();
                        continue;
                    }

                    LoadVariables();
                    ConnectS7Service();
                    if (_pollingThread == null)
                    {
                        _pollingThread = new Thread(PollS7Variable);
                        _pollingThread.IsBackground = true;
                        _pollingThread.Name = "S7ServicePollingThread";
                        _pollingThread.Start();
                    }

                    _reloadEvent.Reset();
                }
            }
            catch (Exception e)
            {
                NlogHelper.Error($"S7后台服务主线程运行中发生了错误:{e.Message}",e);
                DisconnectAllPlc();
            }
        }

        private void PollS7Variable()
        {
            try
            {
                NlogHelper.Info("S7后台服务开始轮询变量....");
                while (!_stopEvent.WaitOne(0))
                {
                    Thread.Sleep(S7PollOnceSleepTimeMs);
                    try
                    {
                        // Stopwatch sw = Stopwatch.StartNew();
                        int varCount = 0;
                        // 遍历并读取每个S7变量。
                        foreach (var deviceId in _pollVariableDic.Keys.ToList())
                        {
                            if (!_deviceDic.TryGetValue(deviceId, out var device))
                            {
                                NlogHelper.Warn($"S7服务轮询时在deviceDic中没有找到Id为：{deviceId}的设备");
                                continue;
                            }

                            if (!_s7PlcClientDic.TryGetValue(device.Ip, out var plcClient))
                            {
                                NlogHelper.Warn($"S7服务轮询时没有找到设备I：{deviceId}的初始化好的Plc客户端对象！");
                                continue;
                            }

                            if (!plcClient.IsConnected)
                            {
                                NlogHelper.Warn($"S7服务轮询时设备：{device.Name},没有连接，正在重新连接...");
                                ConnectS7Service();
                                continue;
                            }

                            if (!_pollVariableDic.TryGetValue(deviceId, out var variableList))
                            {
                                NlogHelper.Warn($"S7服务轮询时没有找到设备I：{deviceId},要轮询的变量列表！");
                                continue;
                            }

                            Dictionary<int, DataItem> readVarDic = new Dictionary<int, DataItem>();

                            foreach (var variable in variableList)
                            {
                                // 获取变量的轮询间隔。
                                if (!ServiceHelper.PollingIntervals.TryGetValue(variable.PollLevelType, out var interval))
                                {
                                    NlogHelper.Info($"未知轮询级别 {variable.PollLevelType}，跳过变量 {variable.Name}。");
                                    continue;
                                }

                                // 检查是否达到轮询时间。
                                if ((DateTime.Now - variable.UpdateTime) < interval)
                                    continue; // 未到轮询时间，跳过。

                                try
                                {
                                    readVarDic.Add(variable.Id, DataItem.FromAddress(variable.S7Address));
                                    varCount++;
                                    if (readVarDic.Count == S7PollOnceReadMultipleVars)
                                    {
                                        // 批量读取
                                        plcClient.ReadMultipleVars(readVarDic.Values.ToList());
                                        // 批量读取后还原结果
                                        foreach (var varId in readVarDic.Keys.ToList())
                                        {
                                            DataItem dataItem = readVarDic[varId];
                                            if (!_variableDic.TryGetValue(varId, out var variableData))
                                            {
                                                NlogHelper.Warn($"S7后台服务批量读取变量后，还原值，在_variableDic中找不到ID为{varId}的变量");
                                                continue;
                                            }

                                            // 更新变量的原始数据值和显示值。
                                            variableData.DataValue = dataItem.Value.ToString();
                                            variableData.UpdateTime = DateTime.Now;
                                            Console.WriteLine($"S7轮询变量:{variableData.Name},值：{variableData.DataValue}");
                                        }

                                        readVarDic.Clear();
                                    }

                                }
                                catch (Exception ex)
                                {
                                    NlogHelper.Warn($"从设备 {device.Name} 读取变量 {variable.Name} 失败:{ex.Message}");
                                }
                            }
                        }

                        // sw.Stop();
                        // Console.WriteLine(
                        //     $"S7轮询设备数：{_pollVariableDic.Count},共轮询变量数：{varCount},总耗时：{sw.ElapsedMilliseconds}ms");
                    }
                    catch (Exception ex)
                    {
                        NotificationHelper.ShowError($"S7后台服务在轮询变量过程中发生错误：{ex.Message}", ex);
                    }
                }

                NlogHelper.Info("S7后台服务轮询变量结束。");
            }
            catch (Exception e)
            {
                NlogHelper.Error($"S7后台服务轮询变量线程运行中发生了错误:{e.Message}",e);
                DisconnectAllPlc();
            }
        }

        private void ConnectS7Service()
        {
            try
            {
                // 检查字典中是否已存在该设备的PLC客户端。
                foreach (var device in _deviceDic.Values.ToList())
                {
                    if (_s7PlcClientDic.TryGetValue(device.Ip, out var plc))
                    {
                        NlogHelper.Info($"已连接到 OPC UA 服务器: {device.Ip}:{device.Prot}");
                        continue;
                    }

                    // 如果不存在或者没有连接，则创建新的Plc客户端。
                    var plcClient = new Plc(device.CpuType, device.Ip, (short)device.Prot, device.Rack, device.Slot);
                    plcClient.Open(); // 尝试打开连接。
                    // 将新创建的客户端添加到字典。
                    if (_s7PlcClientDic.ContainsKey(device.Ip))
                    {
                        _s7PlcClientDic[device.Ip] = plcClient;
                    }
                    else
                    {
                        _s7PlcClientDic.Add(device.Ip, plcClient);
                    }

                    NotificationHelper.ShowSuccess($"已连接到S7 PLC: {device.Name} ({device.Ip})");
                }
            }
            catch (Exception e)
            {
                NotificationHelper.ShowError($"S7服务连接PLC的过程中发生错误：{e}");
            }
        }

        private void LoadVariables()
        {
            try
            {
                _deviceDic.Clear();
                _pollVariableDic.Clear();

                NlogHelper.Info("开始加载S7变量....");
                var _s7Devices = _dataServices
                                 .Devices.Where(d => d.IsActive == true && d.ProtocolType == ProtocolType.S7)
                                 .ToList();
                int varCount = 0;
                foreach (var device in _s7Devices)
                {
                    device.IsRuning = true;
                    _deviceDic.Add(device.Id, device);
                    // 过滤出当前设备和S7协议相关的变量。
                    var s7Variables = device.VariableTables
                                            .Where(vt => vt.ProtocolType == ProtocolType.S7 && vt.IsActive)
                                            .SelectMany(vt => vt.DataVariables)
                                            .Where(vd => vd.IsActive == true)
                                            .ToList();
                    // 将变量存储到字典中，方便以后通过ID快速查找
                    foreach (var s7Variable in s7Variables)
                    {
                        if (_variableDic.ContainsKey(s7Variable.Id))
                        {
                            _variableDic[s7Variable.Id] = s7Variable;
                        }
                        else
                        {
                            _variableDic.Add(s7Variable.Id, s7Variable);
                        }
                    }

                    varCount += s7Variables.Count();
                    _pollVariableDic.Add(device.Id, s7Variables);
                }

                NlogHelper.Info($"S7变量加载成功，共加载S7设备：{_s7Devices.Count}个，变量数：{varCount}");
            }
            catch (Exception e)
            {
                NotificationHelper.ShowError($"S7后台服务加载变量时发生了错误：{e.Message}", e);
            }
        }


        /// <summary>
        /// 停止S7后台服务。
        /// </summary>
        public void StopService()
        {
            NlogHelper.Info("S7后台服务正在关闭....");
            _stopEvent.Set();
            
            _pollingThread.Interrupt();
            _serviceMainThread.Interrupt();
            DisconnectAllPlc();
            
            foreach (Device device in _deviceDic.Values.ToList())
            {
                device.IsRuning = false;
            }
            // 关闭事件
            _reloadEvent.Close();
            _stopEvent.Reset();
            _stopEvent.Close();
            // 清空所有字典。
            _deviceDic.Clear();
            _s7PlcClientDic.Clear();
            _pollVariableDic.Clear();
            NlogHelper.Info("S7后台服务已关闭");
        }

        private void DisconnectAllPlc()
        {
            if (_s7PlcClientDic==null || _s7PlcClientDic.Count == 0)
                return;
            // 关闭所有活跃的PLC连接。
            foreach (var plcClient in _s7PlcClientDic.Values.ToList())
            {
                if (plcClient.IsConnected)
                {
                    plcClient.Close();
                    NlogHelper.Info($"关闭S7连接: {plcClient.IP}");
                }
            }
        }
    }
}