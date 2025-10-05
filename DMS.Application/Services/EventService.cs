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

    #region 变量表事件
    /// <summary>
    /// 变量表改变事件
    /// </summary>
    public event EventHandler<VariableTableChangedEventArgs> OnVariableTableChanged;

    /// <summary>
    /// 触发变量表改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">变量表改变事件参数</param>
    public void RaiseVariableTableChanged(object sender, VariableTableChangedEventArgs e)
    {
        OnVariableTableChanged?.Invoke(sender, e);
    }

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
    /// 批量导入变量事件
    /// </summary>
    public event EventHandler<BatchImportVariablesEventArgs> OnBatchImportVariables;

    /// <summary>
    /// 触发批量导入变量事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">批量导入变量事件参数</param>
    public void RaiseBatchImportVariables(object sender, BatchImportVariablesEventArgs e)
    {
        OnBatchImportVariables?.Invoke(sender, e);
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
    /// MQTT服务器改变事件
    /// </summary>
    public event EventHandler<MqttServerChangedEventArgs> OnMqttServerChanged;

    /// <summary>
    /// 触发MQTT服务器改变事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">MQTT服务器改变事件参数</param>
    public void RaiseMqttServerChanged(object sender, MqttServerChangedEventArgs e)
    {
        OnMqttServerChanged?.Invoke(sender, e);
    }

    #endregion

    #region 数据加载事件

    /// <summary>
    /// 数据加载完成事件
    /// </summary>
    public event EventHandler<DataLoadCompletedEventArgs> OnLoadDataCompleted;

    /// <summary>
    /// 触发数据加载完成事件
    /// </summary>
    /// <param name="sender">事件发送者</param>
    /// <param name="e">数据加载完成事件参数</param>
    public void RaiseLoadDataCompleted(object sender, DataLoadCompletedEventArgs e)
    {
        OnLoadDataCompleted?.Invoke(sender, e);
    }

    #endregion
}