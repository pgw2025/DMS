using DMS.Application.DTOs;
using DMS.Application.Models;
using DMS.Core.Models;
using DMS.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using S7.Net;
using S7.Net.Types;
using CpuType = DMS.Core.Enums.CpuType;
using DateTime = System.DateTime;

namespace DMS.Infrastructure.Services.S7
{
    /// <summary>
    /// S7设备代理类，专门负责与单个S7 PLC进行所有通信
    /// </summary>
    public class S7DeviceAgent : IAsyncDisposable
    {
        private readonly Device _deviceConfig;
        private readonly IChannelBus _channelBus;
        private readonly IMessenger _messenger;
        private readonly ILogger<S7DeviceAgent> _logger;
        private Plc _plc;
        private bool _isConnected;
        private readonly Dictionary<int, List<Variable>> _variablesByPollingInterval;
        private readonly Dictionary<int, DateTime> _lastPollTimes;

        public S7DeviceAgent(Device device, IChannelBus channelBus, IMessenger messenger, ILogger<S7DeviceAgent> logger)
        {
            _deviceConfig = device ?? throw new ArgumentNullException(nameof(device));
            _channelBus = channelBus ?? throw new ArgumentNullException(nameof(channelBus));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _variablesByPollingInterval = new Dictionary<int, List<Variable>>();
            _lastPollTimes = new Dictionary<int, DateTime>();
            
            InitializePlc();
        }

        private void InitializePlc()
        {
            try
            {
                var cpuType = ConvertCpuType(_deviceConfig.CpuType);
                _plc = new Plc(cpuType, _deviceConfig.IpAddress, (short)_deviceConfig.Port, _deviceConfig.Rack, _deviceConfig.Slot);
                _logger.LogInformation($"S7DeviceAgent: 初始化PLC客户端 {_deviceConfig.Name} ({_deviceConfig.IpAddress})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"S7DeviceAgent: 初始化PLC客户端 {_deviceConfig.Name} ({_deviceConfig.IpAddress}) 失败。");
                throw;
            }
        }

        private global::S7.Net.CpuType ConvertCpuType(CpuType cpuType)
        {
            return cpuType switch
            {
                CpuType.S7200 => global::S7.Net.CpuType.S7200,
                CpuType.S7300 => global::S7.Net.CpuType.S7300,
                CpuType.S7400 => global::S7.Net.CpuType.S7400,
                CpuType.S71200 => global::S7.Net.CpuType.S71200,
                CpuType.S71500 => global::S7.Net.CpuType.S71500,
                _ => global::S7.Net.CpuType.S71200
            };
        }

        /// <summary>
        /// 连接到PLC
        /// </summary>
        public async Task ConnectAsync()
        {
            try
            {
                if (_plc != null && !_isConnected)
                {
                    await _plc.OpenAsync();
                    _isConnected = _plc.IsConnected;
                    
                    if (_isConnected)
                    {
                        _logger.LogInformation($"S7DeviceAgent: 成功连接到设备 {_deviceConfig.Name} ({_deviceConfig.IpAddress})");
                    }
                    else
                    {
                        _logger.LogWarning($"S7DeviceAgent: 无法连接到设备 {_deviceConfig.Name} ({_deviceConfig.IpAddress})");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"S7DeviceAgent: 连接设备 {_deviceConfig.Name} ({_deviceConfig.IpAddress}) 失败。");
                _isConnected = false;
            }
        }

        /// <summary>
        /// 断开PLC连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (_plc != null && _isConnected)
                {
                    _plc.Close();
                    _isConnected = false;
                    _logger.LogInformation($"S7DeviceAgent: 断开设备 {_deviceConfig.Name} ({_deviceConfig.IpAddress}) 连接");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"S7DeviceAgent: 断开设备 {_deviceConfig.Name} ({_deviceConfig.IpAddress}) 连接失败。");
            }
        }

        /// <summary>
        /// 更新设备变量配置
        /// </summary>
        public void UpdateVariables(List<Variable> variables)
        {
            // 清空现有的变量分组
            _variablesByPollingInterval.Clear();
            _lastPollTimes.Clear();

            // 按轮询间隔分组变量
            foreach (var variable in variables)
            {
                if (!_variablesByPollingInterval.ContainsKey(variable.PollingInterval))
                {
                    _variablesByPollingInterval[variable.PollingInterval] = new List<Variable>();
                    _lastPollTimes[variable.PollingInterval] = DateTime.MinValue;
                }
                _variablesByPollingInterval[variable.PollingInterval].Add(variable);
            }

            _logger.LogInformation($"S7DeviceAgent: 更新设备 {_deviceConfig.Name} 的变量配置，共 {variables.Count} 个变量");
        }

        /// <summary>
        /// 轮询设备变量
        /// </summary>
        public async Task PollVariablesAsync()
        {
            if (!_isConnected || _plc == null)
            {
                _logger.LogWarning($"S7DeviceAgent: 设备 {_deviceConfig.Name} ({_deviceConfig.IpAddress}) 未连接，跳过轮询。");
                return;
            }

            try
            {
                // 按轮询间隔依次轮询
                foreach (var kvp in _variablesByPollingInterval)
                {
                    var pollingInterval = kvp.Key;
                    var variables = kvp.Value;

                    // 检查是否到了轮询时间
                    if (ShouldPoll(pollingInterval))
                    {
                        await PollVariablesByLevelAsync(variables, pollingInterval);
                        _lastPollTimes[pollingInterval] = DateTime.Now;
                    }
                }
            }
            catch (Exception ex)
            {
                // _messenger.Send(new LogMessage(LogLevel.Error, ex, $"S7DeviceAgent: 设备 {_deviceConfig.Name} ({_deviceConfig.IpAddress}) 轮询错误。"));
            }
        }

        private bool ShouldPoll(int PollingInterval)
        {
            // 获取轮询间隔
            var interval = GetPollingInterval(PollingInterval);
            
            // 检查是否到了轮询时间
            if (_lastPollTimes.TryGetValue(PollingInterval, out var lastPollTime))
            {
                return DateTime.Now - lastPollTime >= interval;
            }
            
            return true;
        }
        private TimeSpan GetPollingInterval(int pollingInterval)
        {
            return pollingInterval switch
            {
                10 => TimeSpan.FromMilliseconds(10), // TenMilliseconds
                100 => TimeSpan.FromMilliseconds(100), // HundredMilliseconds
                500 => TimeSpan.FromMilliseconds(500), // FiveHundredMilliseconds
                1000 => TimeSpan.FromMilliseconds(1000), // OneSecond
                5000 => TimeSpan.FromMilliseconds(5000), // FiveSeconds
                10000 => TimeSpan.FromMilliseconds(10000), // TenSeconds
                20000 => TimeSpan.FromMilliseconds(20000), // TwentySeconds
                30000 => TimeSpan.FromMilliseconds(30000), // ThirtySeconds
                60000 => TimeSpan.FromMilliseconds(60000), // OneMinute
                180000 => TimeSpan.FromMilliseconds(180000), // ThreeMinutes
                300000 => TimeSpan.FromMilliseconds(300000), // FiveMinutes
                600000 => TimeSpan.FromMilliseconds(600000), // TenMinutes
                1800000 => TimeSpan.FromMilliseconds(1800000), // ThirtyMinutes
                _ => TimeSpan.FromMilliseconds(1000)
            };
        }

        private async Task PollVariablesByLevelAsync(List<Variable> variables, int pollingInterval)
        {
            // 批量读取变量
            var dataItems = new List<DataItem>();
            var variableLookup = new Dictionary<int, Variable>(); // 用于关联DataItem和Variable

            foreach (var variable in variables)
            {
                try
                {
                    var dataItem = DataItem.FromAddress(variable.S7Address);
                    dataItems.Add(dataItem);
                    variableLookup[dataItems.Count - 1] = variable; // 记录索引和变量的对应关系
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"S7DeviceAgent: 解析变量 {variable.Name} ({variable.S7Address}) 地址失败。");
                }
            }

            if (dataItems.Count == 0)
                return;

            try
            {
                // 执行批量读取
                await _plc.ReadMultipleVarsAsync(dataItems);

                // 处理读取结果
                for (int i = 0; i < dataItems.Count; i++)
                {
                    if (variableLookup.TryGetValue(i, out var variable))
                    {
                        var dataItem = dataItems[i];
                        if (dataItem?.Value != null)
                        {
                            // 更新变量值
                            variable.DataValue = dataItem.Value.ToString();
                            variable.DisplayValue = dataItem.Value.ToString();
                            variable.UpdatedAt = DateTime.Now;

                            // 创建VariableDto对象
                            var variableDto = new Variable
                            {
                                Id = variable.Id,
                                Name = variable.Name,
                                S7Address = variable.S7Address,
                                DataValue = variable.DataValue,
                                DisplayValue = variable.DisplayValue,
                                PollingInterval = variable.PollingInterval,
                                Protocol = variable.Protocol,
                                UpdatedAt = variable.UpdatedAt
                                // 可以根据需要添加其他属性
                            };

                            // 发送变量更新消息
                            var variableContext = new VariableContext(variableDto);

                            // 写入通道总线
                            await _channelBus.GetWriter<VariableContext>("DataProcessingQueue").WriteAsync(variableContext);
                        }
                    }
                }

                _logger.LogDebug($"S7DeviceAgent: 设备 {_deviceConfig.Name} 完成 {pollingInterval} 毫秒轮询间隔，共处理 {dataItems.Count} 个变量");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"S7DeviceAgent: 设备 {_deviceConfig.Name} 批量读取变量失败。");
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DisconnectAsync();
            _plc?.Close();
        }
    }
}