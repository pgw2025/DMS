using System;
using System.Collections.Generic;
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
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(1); // 轮询间隔

        public S7BackgroundService(ILogger<S7BackgroundService> logger, DataServices dataServices)
        {
            _logger = logger;
            _dataServices = dataServices;
            _dataServices.OnDeviceListChanged += HandleDeviceListChanged;
        }

        private void HandleDeviceListChanged(List<Device> devices)
        {
            // 当设备列表变化时，更新PLC客户端
            // 这里需要更复杂的逻辑来处理连接的关闭和新连接的建立
            // 简单起见，这里只做日志记录
            _logger.LogInformation("Device list changed. S7 clients might need to be reinitialized.");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("S7 Background Service is starting.");

            stoppingToken.Register(() => _logger.LogInformation("S7 Background Service is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("S7 Background Service is doing background work.");

                await PollS7Devices(stoppingToken);

                try
                {
                    await Task.Delay(_pollingInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // When the stopping token is canceled, a TaskCanceledException is thrown.
                    // We should catch it to exit gracefully.
                }
            }

            _logger.LogInformation("S7 Background Service has stopped.");
        }

        private async Task PollS7Devices(CancellationToken stoppingToken)
        {
            var s7Devices = _dataServices.Devices?.Where(d => d.ProtocolType == ProtocolType.S7 && d.IsActive).ToList();

            if (s7Devices == null || !s7Devices.Any())
            {
                _logger.LogDebug("No active S7 devices found to poll.");
                return;
            }

            foreach (var device in s7Devices)
            {
                if (stoppingToken.IsCancellationRequested) return;

                if (!_s7PlcClients.ContainsKey(device.Id))
                {
                    // Initialize Plc client for the device
                    try
                    {
                        var plc = new Plc(device.CpuType, device.Ip, (short)device.Prot, device.Rack, device.Slot);
                        await plc.OpenAsync();
                        _s7PlcClients[device.Id] = plc;
                        _logger.LogInformation($"Connected to S7 PLC: {device.Name} ({device.Ip})");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to connect to S7 PLC: {device.Name} ({device.Ip})");
                        continue;
                    }
                }

                var plcClient = _s7PlcClients[device.Id];
                if (!plcClient.IsConnected)
                {
                    try
                    {
                        await plcClient.OpenAsync();
                        _logger.LogInformation($"Reconnected to S7 PLC: {device.Name} ({device.Ip})");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to reconnect to S7 PLC: {device.Name} ({device.Ip})");
                        continue;
                    }
                }

                // Filter variables for the current device and S7 protocol
                var s7Variables = device.VariableTables
                                        ?.SelectMany(vt => vt.DataVariables)
                                        .Where(vd => vd.ProtocolType == ProtocolType.S7 && vd.IsActive)
                                        .ToList();

                if (s7Variables == null || !s7Variables.Any())
                {
                    _logger.LogDebug($"No active S7 variables found for device: {device.Name}");
                    continue;
                }

                // Batch read variables
                var addressesToRead = s7Variables.Select(vd => vd.S7Address).ToList();
                if (!addressesToRead.Any()) continue;

                try
                {
                    // S7.Net.Plus library supports ReadMultiple, but it's more complex for different data types.
                    // For simplicity, we'll read them one by one for now, or use a more advanced batch read if all are same type.
                    // A more robust solution would involve grouping by data block and type.

                    // Example of reading multiple items (assuming all are of type DWord for simplicity)
                    // This part needs to be refined based on actual variable types and addresses
                    // var dataItems = addressesToRead.Select(addr => new DataItem { DataType = DataType.DataBlock, VarType = VarType.DWord, StringLength = 1, DB = 1, StartByteAdr = 0 }).ToList();
                    // plcClient.ReadMultiple(dataItems);

                    foreach (var variable in s7Variables)
                    {
                        if (stoppingToken.IsCancellationRequested) return;
                        try
                        {
                            // This is a simplified read. In a real scenario, you'd parse S7Address
                            // to get DataType, DB, StartByteAdr, BitAdr, etc.
                            // For now, assuming S7Address is directly readable by Read method (e.g., "DB1.DBW0")
                            var value = await plcClient.ReadAsync(variable.S7Address);
                            if (value != null)
                            {
                                // Update the variable's DataValue and DisplayValue
                                variable.DataValue = value.ToString();
                                variable.DisplayValue = SiemensHelper.ConvertS7Value(value, variable.DataType, variable.Converstion);
                                _logger.LogDebug($"Read {variable.Name}: {variable.DataValue}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to read variable {variable.Name} from {device.Name}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error during batch read for device {device.Name}");
                }
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
