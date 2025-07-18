using System.Collections.ObjectModel;

using DMS.Core.Enums;


namespace DMS.Models;

/// <summary>
/// 表示设备信息。
/// </summary>
public partial class Device 
{
    /// <summary>
    /// 设备的描述信息。
    /// </summary>
    private string description;

    /// <summary>
    /// 设备的类型。
    /// </summary>
    public DeviceType DeviceType { get; set; }

    /// <summary>
    /// 设备的唯一标识符。
    /// </summary>
    private int id;

    /// <summary>
    /// 设备的IP地址。
    /// </summary>
    private string ip;

    /// <summary>
    /// 表示设备是否处于活动状态。
    /// </summary>
    private bool isActive = true;


    /// <summary>
    /// 表示是否添加默认变量表。
    /// </summary>
    private bool isAddDefVarTable = true;

    /// <summary>
    /// 表示设备是否正在运行。
    /// </summary>
    private bool isRuning;

    /// <summary>
    /// 设备的名称。
    /// </summary>
    private string name;

    /// <summary>
    /// 设备的端口号。
    /// </summary>
    private int prot;

    /// <summary>
    /// PLC的CPU类型。
    /// </summary>
    //private CpuType cpuType;

    /// <summary>
    /// PLC的机架号。
    /// </summary>
    private short rack;

    /// <summary>
    /// PLC的槽号。
    /// </summary>
    private short slot;

    /// <summary>
    /// 设备的通信协议类型。
    /// </summary>
    public ProtocolType ProtocolType { get; set; }

    /// <summary>
    /// OPC UA Endpoint URL。
    /// </summary>
    private string? opcUaEndpointUrl;

    /// <summary>
    /// 设备关联的变量表列表。
    /// </summary>
    public ObservableCollection<VariableTable>? VariableTables { get; set; }
}