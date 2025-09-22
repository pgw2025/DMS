using DMS.Application.Interfaces;

namespace DMS.WPF.Interfaces;

/// <summary>
/// 主数据服务接口。
/// </summary>
public interface IWPFDataService
{
    /// <summary>
    /// 设备数据服务。
    /// </summary>
    IDeviceDataService DeviceDataService { get; }

    /// <summary>
    /// 变量数据服务。
    /// </summary>
    IVariableDataService VariableDataService { get; }
    /// <summary>
    /// 变量表数据服务。
    /// </summary>
    public IVariableTableDataService VariableTableDataService { get; }

    /// <summary>
    /// 菜单数据服务。
    /// </summary>
    IMenuDataService MenuDataService { get; }

    /// <summary>
    /// MQTT数据服务。
    /// </summary>
    IMqttDataService MqttDataService { get; }

    /// <summary>
    /// 日志数据服务。
    /// </summary>
    ILogDataService LogDataService { get; }

    /// <summary>
    /// 触发器数据服务。
    /// </summary>
    ITriggerDataService TriggerDataService { get; }
}