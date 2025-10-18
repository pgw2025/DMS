using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Core.Events;
using DMS.Core.Models;
using Microsoft.Extensions.Logging;

namespace DMS.Application.Services;

/// <summary>
/// 设备监视服务实现类，用于监视设备的状态变化
/// </summary>
public class DeviceMonitoringService : IDeviceMonitoringService, IDisposable
{
    private readonly ILogger<DeviceMonitoringService> _logger;
    private readonly IEventService _eventService;
    private readonly IAppStorageService _appStorageService;
    private readonly IAppCenterService _appCenterService;


    /// <summary>
    /// 初始化DeviceMonitoringService类的新实例
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="deviceAppService">设备应用服务</param>
    public DeviceMonitoringService(ILogger<DeviceMonitoringService> logger, IEventService eventService,
                                   IAppStorageService appStorageService,
                                   IAppCenterService appCenterService)
    {
        _logger = logger;
        _eventService = eventService;
        _appStorageService = appStorageService;
        _appCenterService = appCenterService;
        _eventService.OnDeviceStateChanged += OnDeviceStateChanged;
    }

    private void OnDeviceStateChanged(object? sender, DeviceStateChangedEventArgs e)
    {
        // 只处理激活状态变化事件
        if (e.StateType == Core.Enums.DeviceStateType.Active)
        {
            if (_appStorageService.Devices.TryGetValue(e.DeviceId, out var device))
            {
                // 更新设备激活状态 - 同时更新数据库和内存
                _ = Task.Run(async () =>
                {
                    await _appCenterService.DeviceManagementService.UpdateDeviceAsync(device);
                });
            }
        }
    }

    public void Dispose()
    {
        _eventService.OnDeviceStateChanged -= OnDeviceStateChanged;
    }
}