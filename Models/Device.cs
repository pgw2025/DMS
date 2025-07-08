using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;
using S7.Net; // Add this using directive

namespace PMSWPF.Models;

/// <summary>
/// 表示设备信息。
/// </summary>
public partial class Device : ObservableObject
{
    /// <summary>
    /// 设备的描述信息。
    /// </summary>
    [ObservableProperty]
    private string description;

    /// <summary>
    /// 设备的类型。
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public DeviceType DeviceType { get; set; }

    /// <summary>
    /// 设备的唯一标识符。
    /// </summary>
    [ObservableProperty]
    private int id;

    /// <summary>
    /// 设备的IP地址。
    /// </summary>
    [ObservableProperty]
    private string ip;

    /// <summary>
    /// 表示设备是否处于活动状态。
    /// </summary>
    [ObservableProperty]
    private bool isActive = true;

    /// <summary>
    /// 表示是否添加默认变量表。
    /// </summary>
    [ObservableProperty]
    private bool isAddDefVarTable = true;

    /// <summary>
    /// 表示设备是否正在运行。
    /// </summary>
    [ObservableProperty]
    private bool isRuning;

    /// <summary>
    /// 设备的名称。
    /// </summary>
    [ObservableProperty]
    private string name;

    /// <summary>
    /// 设备的端口号。
    /// </summary>
    [ObservableProperty]
    private int prot;

    /// <summary>
    /// PLC的CPU类型。
    /// </summary>
    [ObservableProperty]
    private CpuType cpuType;

    /// <summary>
    /// PLC的机架号。
    /// </summary>
    [ObservableProperty]
    private short rack;

    /// <summary>
    /// PLC的槽号。
    /// </summary>
    [ObservableProperty]
    private short slot;

    /// <summary>
    /// 设备的通信协议类型。
    /// </summary>
    public ProtocolType ProtocolType { get; set; }

    /// <summary>
    /// OPC UA Endpoint URL。
    /// </summary>
    [ObservableProperty]
    private string? opcUaEndpointUrl;

    /// <summary>
    /// 设备关联的变量表列表。
    /// </summary>
    public List<VariableTable>? VariableTables { get; set; }
}