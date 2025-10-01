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
    /// 设备状态改变事件（统一事件，处理激活状态和连接状态变化）
    /// </summary>
    event EventHandler<DeviceStateChangedEventArgs> OnDeviceStateChanged;

    /// <summary>
    /// 触发设备状态改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">设备状态改变事件参数</param>
    void RaiseDeviceStateChanged(object sender, DeviceStateChangedEventArgs e);

    /// <summary>
    /// 设备添加事件
    /// </summary>
    event EventHandler<DeviceChangedEventArgs> OnDeviceChanged;

    /// <summary>
    /// 触发设备添加事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">设备变更事件参数</param>
    void RaiseDeviceChanged(object sender, DeviceChangedEventArgs e);

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
    /// 变量表改变事件
    /// </summary>
    event EventHandler<VariableTableChangedEventArgs> OnVariableTableChanged;

    /// <summary>
    /// 触发变量表改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">变量表改变事件参数</param>
    void RaiseVariableTableChanged(object sender, VariableTableChangedEventArgs e);

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
    /// 批量导入变量事件
    /// </summary>
    event EventHandler<BatchImportVariablesEventArgs> OnBatchImportVariables;

    /// <summary>
    /// 触发批量导入变量事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">批量导入变量事件参数</param>
    void RaiseBatchImportVariables(object sender, BatchImportVariablesEventArgs e);

    /// <summary>
    /// 变量启停改变事件
    /// </summary>
    event EventHandler<VariablesActiveChangedEventArgs> OnVariableActiveChanged;
}