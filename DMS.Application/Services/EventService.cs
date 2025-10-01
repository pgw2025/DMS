using System;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Core.Events;

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
    /// 设备状态改变事件（统一事件，处理激活状态和连接状态变化）
    /// </summary>
    public event EventHandler<DeviceStateChangedEventArgs> OnDeviceStateChanged;

    /// <summary>
    /// 设备添加事件
    /// </summary>
    public event EventHandler<DeviceChangedEventArgs> OnDeviceChanged;


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
                // 转发到统一的设备状态事件
                var unifiedEvent = new DeviceStateChangedEventArgs(e.DeviceId, e.DeviceName, e.NewStatus, Core.Enums.DeviceStateType.Active);
                OnDeviceStateChanged?.Invoke(sender, unifiedEvent);
            }
        }
    }

    /// <summary>
    /// 触发设备连接状态改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">设备状态改变事件参数</param>
    public void RaiseDeviceConnectChanged(object sender, DeviceConnectChangedEventArgs e)
    {
        // 转发到统一的设备状态事件
        var unifiedEvent = new DeviceStateChangedEventArgs(e.DeviceId, e.DeviceName, e.NewStatus, Core.Enums.DeviceStateType.Connection);
        OnDeviceStateChanged?.Invoke(sender, unifiedEvent);
    }

    /// <summary>
    /// 触发设备状态改变事件（统一事件）
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">设备状态改变事件参数</param>
    public void RaiseDeviceStateChanged(object sender, DeviceStateChangedEventArgs e)
    {
        OnDeviceStateChanged?.Invoke(sender, e);
    }

    /// <summary>
    /// 触发设备添加事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">设备变更事件参数</param>
    public void RaiseDeviceChanged(object sender, DeviceChangedEventArgs e)
    {
        OnDeviceChanged?.Invoke(sender, e);
    }

    #endregion

    #region 变量事件
    /// <summary>
    /// 变量值改变事件
    /// </summary>
    public event EventHandler<VariableChangedEventArgs> OnVariableChanged;
    
    /// <summary>
    /// 触发变量值改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">变量值改变事件参数</param>
    public void RaiseVariableChanged(object sender, VariableChangedEventArgs e)
    {
        OnVariableChanged?.Invoke(sender, e);
    }

    
    /// <summary>
    /// 变量启停改变事件
    /// </summary>
    public event EventHandler<VariablesActiveChangedEventArgs> OnVariableActiveChanged;
    public void RaiseVariableActiveChanged(object sender, VariablesActiveChangedEventArgs e)
    {
        OnVariableActiveChanged?.Invoke(sender, e);
    }


    /// <summary>
    /// 变量值改变事件
    /// </summary>
    public event EventHandler<VariableValueChangedEventArgs> OnVariableValueChanged;

    /// <summary>
    /// 触发变量值改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">变量值改变事件参数</param>
    public void RaiseVariableValueChanged(object sender, VariableValueChangedEventArgs e)
    {
        OnVariableValueChanged?.Invoke(sender, e);
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