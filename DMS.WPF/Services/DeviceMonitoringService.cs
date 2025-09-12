using System;
using DMS.WPF.Events;
using DMS.WPF.Interfaces;
using Microsoft.Extensions.Logging;

namespace DMS.WPF.Services;

/// <summary>
/// 设备监控服务，用于监听设备状态改变事件并进行相应处理
/// </summary>
public class DeviceMonitoringService
{
    private readonly ILogger<DeviceMonitoringService> _logger;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// 初始化DeviceMonitoringService类的新实例
    /// </summary>
    /// <param name="logger">日志服务</param>
    /// <param name="eventService">事件服务</param>
    /// <param name="notificationService">通知服务</param>
    public DeviceMonitoringService(
        ILogger<DeviceMonitoringService> logger,
        IEventService eventService,
        INotificationService notificationService)
    {
        _logger = logger;
        _eventService = eventService;
        _notificationService = notificationService;

        // 订阅设备状态改变事件
        _eventService.DeviceStatusChanged += OnDeviceStatusChanged;
    }

    /// <summary>
    /// 处理设备状态改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">设备状态改变事件参数</param>
    private void OnDeviceStatusChanged(object sender, DeviceActiveChangedEventArgs e)
    {
        try
        {
            // 记录设备状态改变日志
            _logger.LogInformation($"设备 {e.DeviceName}(ID: {e.DeviceId}) 状态改变: {e.OldStatus} -> {e.NewStatus}");

            // 根据设备状态改变发送通知
            string message = e.NewStatus 
                ? $"设备 {e.DeviceName} 已启动" 
                : $"设备 {e.DeviceName} 已停止";

            _notificationService.ShowInfo(message);

            // 在这里可以添加更多处理逻辑，例如：
            // 1. 更新数据库中的设备状态历史记录
            // 2. 发送邮件或短信通知
            // 3. 触发其他相关操作
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"处理设备 {e.DeviceName} 状态改变事件时发生错误");
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        // 取消订阅事件
        _eventService.DeviceStatusChanged -= OnDeviceStatusChanged;
    }
}