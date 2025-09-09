using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// 优化后的OPC UA后台服务
    /// </summary>
    public class OptimizedOpcUaBackgroundService : BackgroundService
    {
        private readonly IAppDataCenterService _appDataCenterService;
        private readonly IOpcUaServiceManager _opcUaServiceManager;
        private readonly ILogger<OptimizedOpcUaBackgroundService> _logger;
        private readonly SemaphoreSlim _reloadSemaphore = new SemaphoreSlim(0);

        public OptimizedOpcUaBackgroundService(
            IAppDataCenterService appDataCenterService,
            IOpcUaServiceManager opcUaServiceManager,
            ILogger<OptimizedOpcUaBackgroundService> logger)
        {
            _appDataCenterService = appDataCenterService ?? throw new ArgumentNullException(nameof(appDataCenterService));
            _opcUaServiceManager = opcUaServiceManager ?? throw new ArgumentNullException(nameof(opcUaServiceManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _appDataCenterService.OnLoadDataCompleted += OnLoadDataCompleted;
        }

        private void OnLoadDataCompleted(object sender, DataLoadCompletedEventArgs e)
        {
            _logger.LogInformation("收到数据加载完成通知，触发OPC UA服务重新加载");
            _reloadSemaphore.Release();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("优化后的OPC UA后台服务正在启动...");

            try
            {
                // 初始化服务管理器
                await _opcUaServiceManager.InitializeAsync(stoppingToken);

                while (!stoppingToken.IsCancellationRequested)
                {
                    await _reloadSemaphore.WaitAsync(stoppingToken);

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    if (_appDataCenterService.Devices.IsEmpty)
                    {
                        _logger.LogInformation("没有可用的OPC UA设备，等待设备列表更新...");
                        continue;
                    }

                    await LoadAndConnectDevicesAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("OPC UA后台服务已收到停止请求");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OPC UA后台服务运行时发生未处理的异常: {ErrorMessage}", ex.Message);
            }
            finally
            {
                _logger.LogInformation("正在清理OPC UA后台服务资源...");
                // 服务管理器会在Dispose时自动清理资源
            }

            _logger.LogInformation("优化后的OPC UA后台服务已停止");
        }

        /// <summary>
        /// 加载并连接设备
        /// </summary>
        private async Task LoadAndConnectDevicesAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("开始加载和连接OPC UA设备...");

            try
            {
                // 获取所有活动的OPC UA设备
                var opcUaDevices = _appDataCenterService.Devices.Values
                    .Where(d => d.Protocol == ProtocolType.OpcUa && d.IsActive)
                    .ToList();

                _logger.LogInformation("找到 {DeviceCount} 个活动的OPC UA设备", opcUaDevices.Count);

                if (opcUaDevices.Count == 0)
                    return;

                // 添加设备到监控列表
                foreach (var device in opcUaDevices)
                {
                    _opcUaServiceManager.AddDevice(device);

                    // 获取设备变量
                    var variables = device.VariableTables?
                        .SelectMany(vt => vt.Variables)
                        .Where(v => v.IsActive && v.Protocol == ProtocolType.OpcUa)
                        .ToList() ?? new List<VariableDto>();

                    _opcUaServiceManager.UpdateVariables(device.Id, variables);
                }

                // 批量连接设备
                var deviceIds = opcUaDevices.Select(d => d.Id).ToList();
                await _opcUaServiceManager.ConnectDevicesAsync(deviceIds, stoppingToken);


                _logger.LogInformation("OPC UA设备加载和连接完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载和连接OPC UA设备时发生错误: {ErrorMessage}", ex.Message);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("正在停止OPC UA后台服务...");

            // 释放信号量以确保ExecuteAsync可以退出
            _reloadSemaphore.Release();

            await base.StopAsync(cancellationToken);
            
            _logger.LogInformation("OPC UA后台服务停止完成");
        }

        public override void Dispose()
        {
            _logger.LogInformation("正在释放OPC UA后台服务资源...");
            
            _appDataCenterService.OnLoadDataCompleted -= OnLoadDataCompleted;
            _reloadSemaphore?.Dispose();
            
            base.Dispose();
            
            _logger.LogInformation("OPC UA后台服务资源已释放");
        }
    }
}