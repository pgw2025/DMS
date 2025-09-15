using DMS.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Logging;
using NLog;
using S7.Net;
using S7.Net.Types;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// S7服务实现类，用于与S7 PLC进行通信
    /// </summary>
    public class S7Service : IS7Service
    {
        private const int ReadMultipleVarsCount = 10;
        
        private Plc _plc;
        private readonly ILogger<S7Service> _logger;

        public bool IsConnected => _plc?.IsConnected ?? false;

        public S7Service(ILogger<S7Service> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 异步连接到S7 PLC
        /// </summary>
        public async Task ConnectAsync(string ipAddress, int port, short rack, short slot, CpuType cpuType)
        {
            try
            {
                _plc = new Plc(cpuType, ipAddress, (short)port, rack, slot);
                await _plc.OpenAsync();
                _logger.LogInformation($"成功连接到S7 PLC: {ipAddress}:{port}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"连接S7 PLC时发生错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 异步断开与S7 PLC的连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (_plc != null)
                {
                    _plc.Close();
                    _logger.LogInformation("已断开S7 PLC连接");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"断开S7 PLC连接时发生错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 异步读取单个变量的值
        /// </summary>
        public async Task<object> ReadVariableAsync(string address)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("PLC未连接");
            }

            try
            {
                var dataItem = DataItem.FromAddress(address);
                // await _plc.ReadMultipleVarsAsync(dataItem);
                return dataItem.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取变量 {address} 时发生错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 异步读取多个变量的值
        /// </summary>
        public async Task<Dictionary<string, object>> ReadVariablesAsync(List<string> addresses)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("PLC未连接");
            }
            

            try
            { 
                var result = new Dictionary<string, object>();
                var dataItems = addresses.Select(DataItem.FromAddress).ToList();

                await _plc.ReadMultipleVarsAsync(dataItems);

               
                for (int i = 0; i < addresses.Count; i++)
                {
                    result[addresses[i]] = dataItems[i].Value;
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批量读取变量时发生错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 异步写入单个变量的值
        /// </summary>
        public async Task WriteVariableAsync(string address, object value)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("PLC未连接");
            }

            try
            {
                await _plc.WriteAsync(address, value);
                _logger.LogInformation($"成功写入变量 {address}，值: {value}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"写入变量 {address} 时发生错误: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 异步写入多个变量的值
        /// </summary>
        public async Task WriteVariablesAsync(Dictionary<string, object> values)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("PLC未连接");
            }

            try
            {
                var addresses = values.Keys.ToList();
                var dataItems = new List<DataItem>();

                foreach (var kvp in values)
                {
                    var dataItem = DataItem.FromAddress(kvp.Key);
                    dataItem.Value = kvp.Value;
                    dataItems.Add(dataItem);
                }

                // await _plc.write(dataItems);
                _logger.LogInformation($"成功批量写入 {values.Count} 个变量");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"批量写入变量时发生错误: {ex.Message}");
                throw;
            }
        }
    }
}