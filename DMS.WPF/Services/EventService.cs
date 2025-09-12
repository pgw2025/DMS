using System;
using DMS.WPF.Events;
using DMS.WPF.Interfaces;

namespace DMS.WPF.Services;

/// <summary>
/// 事件服务实现类，用于统一管理应用程序中的各种事件
/// </summary>
public class EventService : IEventService
{
    #region 设备事件

    /// <summary>
    /// 设备状态改变事件
    /// </summary>
    public event EventHandler<DeviceActiveChangedEventArgs> DeviceStatusChanged;

    /// <summary>
    /// 触发设备状态改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">设备状态改变事件参数</param>
    public void RaiseDeviceStatusChanged(object sender, DeviceActiveChangedEventArgs e)
    {
        DeviceStatusChanged?.Invoke(sender, e);
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