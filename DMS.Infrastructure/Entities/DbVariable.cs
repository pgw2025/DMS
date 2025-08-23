using DMS.Core.Enums;
using SqlSugar;
using SqlSugar.DbConvert;
using CSharpDataType = SqlSugar.CSharpDataType;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 代表数据库中的变量实体，与 'variable' 表对应。
/// </summary>
public class DbVariable
{
    /// <summary>
    /// 主键ID，自增长。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 变量名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 变量的描述信息，可以为空。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string Description { get; set; }

    /// <summary>
    /// 变量的信号类型（例如：状态、控制、报警等）。
    /// </summary>
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public SignalType SignalType { get; set; }

    /// <summary>
    /// 变量的轮询级别，决定数据采集频率。
    /// </summary>
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public PollLevelType PollLevel { get; set; }

    /// <summary>
    /// 指示此变量是否处于激活状态。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 所属变量表的ID (外键)。
    /// </summary>
    public int VariableTableId { get; set; }

    /// <summary>
    /// 从设备读取到的原始值。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string DataValue { get; set; }

    /// <summary>
    /// 经过转换公式计算后的显示值。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string DisplayValue { get; set; }

    /// <summary>
    /// S7协议中的地址 (例如: DB1.DBD0, M100.0)，可以为空。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string S7Address { get; set; }

    /// <summary>
    /// OPC UA协议中的NodeId，可以为空。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string OpcUaNodeId { get; set; }

    /// <summary>
    /// 是否启用历史数据记录。
    /// </summary>
    public bool IsHistoryEnabled { get; set; }

    /// <summary>
    /// 历史数据记录的死区值，变化量超过该值时才记录。
    /// </summary>
    public double HistoryDeadband { get; set; }

    /// <summary>
    /// 是否启用报警功能。
    /// </summary>
    public bool IsAlarmEnabled { get; set; }

    /// <summary>
    /// 报警下限值。
    /// </summary>
    public double AlarmMinValue { get; set; }

    /// <summary>
    /// 报警上限值。
    /// </summary>
    public double AlarmMaxValue { get; set; }

    /// <summary>
    /// 报警死区值，变化量超过该值时才触发报警。
    /// </summary>
    public double AlarmDeadband { get; set; }

    /// <summary>
    /// 变量使用的通讯协议类型。
    /// </summary>
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public ProtocolType Protocol { get; set; }

    /// <summary>
    /// 变量的数据类型。
    /// </summary>
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public CSharpDataType CSharpDataType { get; set; }

    /// <summary>
    /// 数值转换公式 (例如: "+3*5")，可以为空。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string ConversionFormula { get; set; }

    /// <summary>
    /// 记录创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 记录最后更新时间。
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 最后更新记录的用户名或系统进程名。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string UpdatedBy { get; set; }

    /// <summary>
    /// 标记该记录是否被修改，用于同步。
    /// </summary>
    public bool IsModified { get; set; }

    /// <summary>
    /// OPC UA的更新类型（例如：轮询、订阅）。
    /// </summary>
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public OpcUaUpdateType OpcUaUpdateType { get; set; }
}