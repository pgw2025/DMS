using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;
using ProtocolType = PMSWPF.Enums.ProtocolType;

namespace PMSWPF.Data.Entities;

/// <summary>
/// 表示数据库中的设备实体。
/// </summary>
[SugarTable("Device")]
public class DbDevice
{
    /// <summary>
    /// 设备的描述信息。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 设备的类型。
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public DeviceType DeviceType { get; set; }

    /// <summary>
    /// 设备的唯一标识符。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增 
    public int Id { get; set; }

    /// <summary>
    /// 设备的IP地址。
    /// </summary>
    public string Ip { get; set; }

    /// <summary>
    /// 表示设备是否处于活动状态。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 表示设备是否正在运行。
    /// </summary>
    public bool IsRuning { get; set; }

    /// <summary>
    /// 设备的名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 设备的通信协议类型。
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public ProtocolType ProtocolType { get; set; }

    /// <summary>
    /// 设备关联的变量表列表。
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(DbVariableTable.DeviceId))]
    [SugarColumn(IsNullable = true)]
    public List<DbVariableTable>? VariableTables { get; set; }
}