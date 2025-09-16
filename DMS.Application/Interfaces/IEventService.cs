using System;
using DMS.Application.Events;
using DMS.Core.Events;

namespace DMS.Application.Interfaces;

/// <summary>
/// 事件服务接口，用于统一管理应用程序中的各种事件
/// </summary>
public interface IEventService
{
    #region 设备事件

    /// <summary>
    /// 设备状态改变事件
    /// </summary>
    event EventHandler<DeviceActiveChangedEventArgs> OnDeviceActiveChanged;

    /// <summary>
    /// 触发设备状态改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">设备状态改变事件参数</param>
    void RaiseDeviceActiveChanged(object sender, DeviceActiveChangedEventArgs e);

    #endregion

    #region 变量事件

    /// <summary>
    /// 变量值改变事件
    /// </summary>
    event EventHandler<VariableValueChangedEventArgs> OnVariableValueChanged;

    /// <summary>
    /// 触发变量值改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">变量值改变事件参数</param>
    void RaiseVariableValueChanged(object sender, VariableValueChangedEventArgs e);

    #endregion

    #region MQTT事件

    /// <summary>
    /// MQTT连接状态改变事件
    /// </summary>
    event EventHandler<MqttConnectionChangedEventArgs> MqttConnectionChanged;

    /// <summary>
    /// 触发MQTT连接状态改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">MQTT连接状态改变事件参数</param>
    void RaiseMqttConnectionChanged(object sender, MqttConnectionChangedEventArgs e);

    #endregion

    /// <summary>
    /// 设备运行改变事件
    /// </summary>
    event EventHandler<DeviceConnectChangedEventArgs> OnDeviceConnectChanged;

    /// <summary>
    /// 触发设备状态改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">设备状态改变事件参数</param>
    void RaiseDeviceConnectChanged(object sender, DeviceConnectChangedEventArgs e);

    /// <summary>
    /// 变量值改变事件
    /// </summary>
    event EventHandler<VariableChangedEventArgs> OnVariableChanged;

    /// <summary>
    /// 触发变量值改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">变量值改变事件参数</param>
    void RaiseVariableChanged(object sender, VariableChangedEventArgs e);

    void RaiseVariableActiveChanged(object sender,VariablesActiveChangedEventArgs e);

    /// <summary>
    /// 变量启停改变事件
    /// </summary>
    event EventHandler<VariablesActiveChangedEventArgs> OnVariableActiveChanged;
}