using System;
using DMS.Application.Events;
using DMS.Application.Interfaces;

namespace DMS.Application.Services;

/// <summary>
/// 事件服务实现类，用于统一管理应用程序中的各种事件
/// </summary>
public class EventService : IEventService
{
    private readonly IAppDataStorageService _appDataStorageService;

    public EventService(IAppDataStorageService appDataStorageService)
    {
        _appDataStorageService = appDataStorageService;
    }

    #region 设备事件

    /// <summary>
    /// 设备状态改变事件
    /// </summary>
    public event EventHandler<DeviceActiveChangedEventArgs> OnDeviceActiveChanged;
    
    /// <summary>
    /// 设备运行改变事件
    /// </summary>
    public event EventHandler<DeviceConnectChangedEventArgs> OnDeviceConnectChanged;


    /// <summary>
    /// 触发设备状态改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">设备状态改变事件参数</param>
    public void RaiseDeviceActiveChanged(object sender, DeviceActiveChangedEventArgs e)
    {
        if (_appDataStorageService.Devices.TryGetValue(e.DeviceId, out var device))
        {
            if (device.IsActive != e.NewStatus)
            {
                device.IsActive = e.NewStatus;
                OnDeviceActiveChanged?.Invoke(sender, e);
            }
        }
    }

    /// <summary>
    /// 触发设备状态改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">设备状态改变事件参数</param>
    public void RaiseDeviceConnectChanged(object sender, DeviceConnectChangedEventArgs e)
    {
        OnDeviceConnectChanged?.Invoke(sender, e);

    }

    #endregion

    #region 变量事件

    /// <summary>
    /// 变量值改变事件
    /// </summary>
    public event EventHandler<VariableValueChangedEventArgs> VariableValueChanged;

    /// <summary>
    /// 触发变量值改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">变量值改变事件参数</param>
    public void RaiseVariableValueChanged(object sender, VariableValueChangedEventArgs e)
    {
        VariableValueChanged?.Invoke(sender, e);
    }

    #endregion

    #region MQTT事件

    /// <summary>
    /// MQTT连接状态改变事件
    /// </summary>
    public event EventHandler<MqttConnectionChangedEventArgs> MqttConnectionChanged;

    /// <summary>
    /// 触发MQTT连接状态改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">MQTT连接状态改变事件参数</param>
    public void RaiseMqttConnectionChanged(object sender, MqttConnectionChangedEventArgs e)
    {
        MqttConnectionChanged?.Invoke(sender, e);
    }

    #endregion
}