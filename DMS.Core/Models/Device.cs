using System.Collections.ObjectModel;

using DMS.Core.Enums;


namespace DMS.Core.Models;

/// <summary>
/// 表示设备信息。
/// </summary>
public partial class Device 
{
    /// <summary>
    /// 设备的描述信息。
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 设备的类型。
    /// </summary>
    public DeviceType DeviceType { get; set; }

    /// <summary>
    /// 设备的唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 设备的IP地址。
    /// </summary>
    public string Ip { get; set; }

    /// <summary>
    /// 表示设备是否处于活动状态。
    /// </summary>
    public bool IsActive { get; set; } = true;


    /// <summary>
    /// 表示是否添加默认变量表。
    /// </summary>
    public bool IsAddDefVarTable { get; set; } = true;

    /// <summary>
    /// 表示设备是否正在运行。
    /// </summary>
    public bool IsRuning { get; set; }

    /// <summary>
    /// 设备的名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 设备的端口号。
    /// </summary>
    public int Prot { get; set; }

    /// <summary>
    /// PLC的CPU类型。
    /// </summary>
    //public CpuType cpuType;

    /// <summary>
    /// PLC的机架号。
    /// </summary>
    public short Rack { get; set; }

    /// <summary>
    /// PLC的槽号。
    /// </summary>
    public short Slot { get; set; }

    /// <summary>
    /// 设备的通信协议类型。
    /// </summary>
    public ProtocolType ProtocolType { get; set; }

    /// <summary>
    /// OPC UA Endpoint URL。
    /// </summary>
    public string OpcUaEndpointUrl { get; set; }=String.Empty;

    /// <summary>
    /// 设备关联的变量表列表。
    /// </summary>
    public ObservableCollection<VariableTable>? VariableTables { get; set; }
}