using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Models;

namespace PMSWPF.Services
{
    public class OpcUaBackgroundService
    {
        private readonly DataServices _dataServices;

        // 存储 OPC UA 设备，键为设备Id，值为会话对象。
        private readonly Dictionary<int, Device> _deviceDic;

        // 存储 OPC UA 会话，键为终结点 URL，值为会话对象。
        private readonly Dictionary<string, Session> _sessionsDic;

        // 存储 OPC UA 订阅，键为终结点 URL，值为订阅对象。
        private readonly Dictionary<string, Subscription> _subscriptionsDic;

        // 存储活动的 OPC UA 变量，键为变量的OpcNodeId
        private readonly Dictionary<string, VariableData> _opcUaNodeIdVariableDic; // Key: VariableData.Id

        // 储存所有要轮询更新的变量，键是Device.Id,值是这个设备所有要轮询的变量
        private readonly Dictionary<int, List<VariableData>> _pollVariableDic; // Key: VariableData.Id

        // 储存所有要订阅更新的变量，键是Device.Id,值是这个设备所有要轮询的变量
        private readonly Dictionary<int, List<VariableData>> _subVariableDic;


       
        // 后台服务的主线程，负责连接服务器，加载变量，订阅变量
        private Thread _serviceMainThread;

        // 轮询线程
        private Thread _pollThread;

        // 重新加载事件
        private readonly ManualResetEvent _reloadEvent = new ManualResetEvent(false);

        // 停止事件,触发后会停止整个Opc后台服务
        private readonly ManualResetEvent _stopdEvent = new ManualResetEvent(false);


        /// <summary>
        /// OpcUaBackgroundService 的构造函数。
        /// </summary>
        /// <param name="dataServices">数据服务，用于访问数据库中的变量信息。</param>
        public OpcUaBackgroundService(DataServices dataServices)
        {
            _dataServices = dataServices;
            _sessionsDic = new Dictionary<string, Session>();
            _subscriptionsDic = new Dictionary<string, Subscription>();
            _opcUaNodeIdVariableDic = new();
            _pollVariableDic = new();
            _subVariableDic = new();
            _deviceDic = new();
        }

        /// <summary>
        /// 后台服务的主执行方法。
        /// </summary>
        /// <param name="stoppingToken">用于通知服务停止的取消令牌。</param>
        /// <returns>表示异步操作的任务。</returns>
        public void StartService()
        {
            NlogHelper.Info("OPC UA 服务正在启动...");
            _reloadEvent.Set();
            _serviceMainThread = new Thread(Execute);
            _serviceMainThread.IsBackground = true;
            _serviceMainThread.Name = "OpcUaServiceThread";
            _serviceMainThread.Start();
        }

        public void StopService()
        {
            NlogHelper.Info("OPC UA 服务正在停止...");
            _stopdEvent.Set();
            DisconnectAllOpcUaSessions();

            _reloadEvent.Close();
            _stopdEvent.Close();
            NlogHelper.Info("OPC UA 服务已经停止。");
        }

        private void Execute()
        {
            // 订阅变量数据变化事件，以便在变量配置发生变化时重新加载。
            _dataServices.OnDeviceListChanged += HandleDeviceListChanged;

            while (!_stopdEvent.WaitOne(0))
            {
                _reloadEvent.WaitOne();

                if (_dataServices.Devices == null || _dataServices.Devices.Count == 0)
                {
                    _reloadEvent.Reset();
                    continue;
                }

                NlogHelper.Info("OpcUa后台服务开始加载变量...");
                // 初始化时加载所有活动的 OPC UA 变量。
                LoadOpcUaVariables();

                //连接服务器
                ConnectOpcUaService();
                // // 添加订阅变量
                SetupOpcUaSubscription();

                if (_pollThread == null)
                {
                    _pollThread = new Thread(PollOpcUaVariable);
                    _pollThread.IsBackground = true;
                    _pollThread.Name = "OpcUaPollThread";
                    _pollThread.Start();
                }

                NlogHelper.Info("OpcUa后台服务已启动。");
                _reloadEvent.Reset();
            }


            // 循环运行，直到接收到停止信号。
            // while (!stoppingToken.IsCancellationRequested)
            // {
            //     // 可以在这里添加周期性任务，例如检查和重新连接断开的会话。
            //     await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            // }
            NlogHelper.Info("OpcUa后台服务正在停止。");
            // 服务停止时，取消订阅事件并断开所有 OPC UA 连接。
            _dataServices.OnDeviceListChanged -= HandleDeviceListChanged;
        }

        private void HandleDeviceListChanged(List<Device> devices)
        {
            NlogHelper.Info("变量数据已更改。正在重新加载 OPC UA 变量。");
            _reloadEvent.Set();
        }

        /// <summary>
        /// 从数据库加载所有活动的 OPC UA 变量，并进行相应的连接和订阅管理。
        /// </summary>
        private void LoadOpcUaVariables()
        {
            try
            {
                var _opcUaDevices = _dataServices
                                    .Devices.Where(d => d.ProtocolType == ProtocolType.OpcUA && d.IsActive == true)
                                    .ToList();

                if (_opcUaDevices.Count == 0)
                    return;
                _deviceDic.Clear();
                _pollVariableDic.Clear();
                _subVariableDic.Clear();
                _opcUaNodeIdVariableDic.Clear();
                foreach (var opcUaDevice in _opcUaDevices)
                {
                    // 将设备保存到字典中，方便之后查找
                    _deviceDic.Add(opcUaDevice.Id, opcUaDevice);
                    //查找设备中所有要轮询的变量
                    var dPollList = opcUaDevice.VariableTables?.SelectMany(vt => vt.DataVariables)
                                               .Where(vd => vd.IsActive == true &&
                                                            vd.ProtocolType == ProtocolType.OpcUA &&
                                                            vd.OpcUaUpdateType == OpcUaUpdateType.OpcUaPoll)
                                               .ToList();
                    // 将变量保存到字典中，方便Read后还原
                    foreach (var variableData in dPollList)
                    {
                        _opcUaNodeIdVariableDic.Add(variableData.OpcUaNodeId, variableData);
                    }

                    NlogHelper.Info($"加载OpcUa轮询变量：{dPollList.Count}个");
                    _pollVariableDic.Add(opcUaDevice.Id, dPollList);
                    //查找设备中所有要订阅的变量
                    var dSubList = opcUaDevice.VariableTables?.SelectMany(vt => vt.DataVariables)
                                              .Where(vd => vd.IsActive == true &&
                                                           vd.ProtocolType == ProtocolType.OpcUA &&
                                                           vd.OpcUaUpdateType == OpcUaUpdateType.OpcUaSubscription)
                                              .ToList();
                    _subVariableDic.Add(opcUaDevice.Id, dSubList);
                    NlogHelper.Info($"加载OpcUa订阅变量：{dSubList.Count}个");
                }
            }
            catch (Exception e)
            {
                NotificationHelper.ShowError($"加载OpcUa变量的过程中发生了错误：{e.Message}");
            }
        }

        /// <summary>
        /// 连接到 OPC UA 服务器并订阅或轮询指定的变量。
        /// </summary>
        private void ConnectOpcUaService()
        {
            foreach (Device device in _deviceDic.Values)
            {
                Session session = null;
                // 检查是否已存在到该终结点的活动会话。
                if (_sessionsDic.TryGetValue(device.OpcUaEndpointUrl, out session) && session.Connected)
                {
                    NlogHelper.Info($"已连接到 OPC UA 服务器: {device.OpcUaEndpointUrl}");
                    continue;
                }

                session = ServiceHelper.CreateOpcUaSession(device.OpcUaEndpointUrl);
                if (session == null)
                    return; // 连接失败，直接返回

                _sessionsDic[device.OpcUaEndpointUrl] = session;
            }
        }

        private void PollOpcUaVariable()
        {
            NlogHelper.Info("OpcUa轮询变量线程已启动，开始轮询变量....");
            while (!_stopdEvent.WaitOne(0))
            {
                try
                {
                    foreach (var deviceId in _pollVariableDic.Keys.ToList())
                    {
                        Thread.Sleep(100);
                        if (!_deviceDic.TryGetValue(deviceId, out var device) || device.OpcUaEndpointUrl == null)
                        {
                            NlogHelper.Warn(
                                $"OpcUa轮询变量时，在deviceDic中未找到ID为 {deviceId} 的设备，或其服务器地址为空，请检查！");
                            continue;
                        }

                        _sessionsDic.TryGetValue(device.OpcUaEndpointUrl, out Session session);
                        if (session == null || !session.Connected)
                        {
                            if (!_stopdEvent.WaitOne(0))
                            {
                                NlogHelper.Warn(
                                    $"用于 {device.OpcUaEndpointUrl} 的 OPC UA 会话未连接。正在尝试重新连接...");
                                // 尝试重新连接会话
                                ConnectOpcUaService();
                                continue;
                            }
                        }

                        var nodesToRead = new ReadValueIdCollection();
                        if (!_pollVariableDic.TryGetValue(deviceId, out var variableList))
                        {
                            continue;
                        }

                        foreach (var variable in variableList)
                        {
                            // 获取变量的轮询间隔。
                            if (!ServiceHelper.PollingIntervals.TryGetValue(variable.PollLevelType, out var interval))
                            {
                                NlogHelper.Info($"未知的轮询级别 {variable.PollLevelType}，跳过变量 {variable.Name}。");
                                continue;
                            }

                            // 检查是否达到轮询时间。
                            if ((DateTime.Now - variable.UpdateTime) < interval)
                                continue; // 未到轮询时间，跳过。

                            nodesToRead.Add(new ReadValueId
                                            {
                                                NodeId = new NodeId(variable.OpcUaNodeId),
                                                AttributeId = Attributes.Value
                                            });
                        }

                        // 如果没有要读取的变量则跳过
                        if (nodesToRead.Count == 0)
                            continue;


                        session.Read(
                            null,
                            0,
                            TimestampsToReturn.Both,
                            nodesToRead,
                            out DataValueCollection results,
                            out DiagnosticInfoCollection diagnosticInfos);

                        if (results == null || results.Count == 0)
                            continue;
                        for (int i = 0; i < results.Count; i++)
                        {
                            var value = results[i];
                            var nodeId = nodesToRead[i]
                                         .NodeId.ToString();
                            if (!_opcUaNodeIdVariableDic.TryGetValue(nodeId, out var variable))
                            {
                                NlogHelper.Warn(
                                    $"在字典中未找到OpcUaNodeId为 {nodeId} 的变量对象！");
                                continue;
                            }

                            if (!StatusCode.IsGood(value.StatusCode))
                            {
                                NlogHelper.Warn(
                                    $"读取 OPC UA 变量 {variable.Name} ({variable.OpcUaNodeId}) 失败: {value.StatusCode}");
                                continue;
                            }


                            // 更新变量数据
                            variable.DataValue = value.Value.ToString();
                            variable.DisplayValue = value.Value.ToString(); // 或者根据需要进行格式化
                            variable.UpdateTime = DateTime.Now;
                            NlogHelper.Info($"轮询变量：{variable.Name},值：{variable.DataValue}");
                            // Console.WriteLine($"结果变量跟更新时间:{variable.UpdateTime}");
                            // await _dataServices.UpdateVariableDataAsync(variable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    NotificationHelper.ShowError($"OPC UA 轮询期间发生错误: {ex.Message}", ex);
                }
            }

            NlogHelper.Info("OpcUa轮询变量线程已停止。");
        }

        /// <summary>
        /// 订阅变量变化的通知
        /// </summary>
        /// <param name="monitoredItem"></param>
        /// <param name="e"></param>
        private void OnSubNotification(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            foreach (var value in monitoredItem.DequeueValues())
            {
                NlogHelper.Info(
                    $"[OPC UA 通知] {monitoredItem.DisplayName}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
                Console.WriteLine(
                    $"[通知] {monitoredItem.DisplayName}: {value.Value} | 时间戳: {value.SourceTimestamp.ToLocalTime()} | 状态: {value.StatusCode}");
            }
        }


        /// <summary>
        /// 断开所有 OPC UA 会话。
        /// </summary>
        private void DisconnectAllOpcUaSessions()
        {
            NlogHelper.Info("正在断开所有 OPC UA 会话...");
            foreach (var endpointUrl in _sessionsDic.Keys.ToList())
            {
                NlogHelper.Info($"正在断开 OPC UA 会话: {endpointUrl}");
                if (_sessionsDic.TryGetValue(endpointUrl, out var session))
                {
                    if (_subscriptionsDic.TryGetValue(endpointUrl, out var subscription))
                    {
                        // 删除订阅。
                        subscription.Delete(true);
                        _subscriptionsDic.Remove(endpointUrl);
                    }


                    // 关闭会话。
                    session.Close();
                    _sessionsDic.Remove(endpointUrl);
                    NotificationHelper.ShowInfo($"已从 OPC UA 服务器断开连接: {endpointUrl}");
                }
            }
        }


        /// <summary>
        /// 设置 OPC UA 订阅并添加监控项。
        /// </summary>
        /// <param name="session">OPC UA 会话。</param>
        /// <param name="variable">要订阅的变量信息。</param>
        /// <param name="endpointUrl">OPC UA 服务器的终结点 URL。</param>
        private void SetupOpcUaSubscription()
        {
            foreach (var deviceId in _subVariableDic.Keys)
            {
                var device = _dataServices.Devices.FirstOrDefault(d => d.Id == deviceId);
                Subscription subscription = null;
                // 得到session
                if (!_sessionsDic.TryGetValue(device.OpcUaEndpointUrl, out var session))
                {
                    NlogHelper.Info($"从OpcUa会话字典中获取会话失败： {device.OpcUaEndpointUrl} ");
                    continue;
                }

                // 判断设备是否已经添加了订阅
                if (_subscriptionsDic.TryGetValue(device.OpcUaEndpointUrl, out subscription))
                {
                    NlogHelper.Info($"OPC UA 终结点 {device.OpcUaEndpointUrl} 已存在订阅。");
                }
                else
                {
                    subscription = new Subscription(session.DefaultSubscription);
                    subscription.PublishingInterval = 1000; // 发布间隔（毫秒）
                    session.AddSubscription(subscription);
                    subscription.Create();
                    _subscriptionsDic[device.OpcUaEndpointUrl] = subscription;
                }

                // 将变量添加到订阅
                foreach (VariableData variable in _subVariableDic[deviceId])
                {
                    // 7. 创建监控项并添加到订阅中。
                    MonitoredItem monitoredItem = new MonitoredItem(subscription.DefaultItem);
                    monitoredItem.DisplayName = variable.Name;
                    monitoredItem.StartNodeId = new NodeId(variable.OpcUaNodeId); // 设置要监控的节点 ID
                    monitoredItem.AttributeId = Attributes.Value; // 监控节点的值属性
                    monitoredItem.SamplingInterval = 1000; // 采样间隔（毫秒）
                    monitoredItem.QueueSize = 1; // 队列大小
                    monitoredItem.DiscardOldest = true; // 丢弃最旧的数据
                    // 注册数据变化通知事件。
                    monitoredItem.Notification += OnSubNotification;

                    subscription.AddItem(monitoredItem);
                    subscription.ApplyChanges(); // 应用更改
                }

                NlogHelper.Info($"设备: {device.Name}, 添加了 {(_subVariableDic[deviceId]?.Count ?? 0)} 个订阅变量。");
            }
        }
    }
}