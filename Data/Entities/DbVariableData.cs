using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace PMSWPF.Data.Entities;

/// <summary>
/// 表示数据库中的变量数据实体。
/// </summary>
[SugarTable("VarData")]
public class DbVariableData
{
    /// <summary>
    /// 变量唯一标识符。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增
    public int Id { get; set; }

    /// <summary>
    /// 变量名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 节点ID，用于标识变量在设备或系统中的唯一路径。
    /// </summary>
    public string NodeId { get; set; } = String.Empty;
    
    /// <summary>
    /// 节点ID，用于标识变量在设备或系统中的唯一路径。
    /// </summary>
    public string S7Address { get; set; } = String.Empty;

    /// <summary>
    /// 变量描述。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 协议类型，例如Modbus、OPC UA等。
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public ProtocolType ProtocolType { get; set; }

    /// <summary>
    /// 信号类型，例如模拟量、数字量等。
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(20)", IsNullable = true, SqlParameterDbType = typeof(EnumToStringConvert))]
    public SignalType SignalType { get; set; }

    /// <summary>
    /// 数据类型，例如Int、Float、String等。
    /// </summary>
    public string DataType { get; set; } = String.Empty;

    /// <summary>
    /// 变量当前原始数据值。
    /// </summary>
    public string DataValue { get; set; } = String.Empty;

    /// <summary>
    /// 变量经过转换或格式化后的显示值。
    /// </summary>
    public string DisplayValue { get; set; } = String.Empty;

    /// <summary>
    /// 指示变量是否处于激活状态。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 指示是否需要保存变量数据。
    /// </summary>
    public bool IsSave { get; set; }

    /// <summary>
    /// 指示是否需要对变量进行报警监测。
    /// </summary>
    public bool IsAlarm { get; set; }

    /// <summary>
    /// 轮询级别，例如1秒、5秒等。
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public PollLevelType PollLevelType { get; set; }

    /// <summary>
    /// 指示变量是否已被逻辑删除。
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 报警的最大值阈值。
    /// </summary>
    public double AlarmMax { get; set; }

    /// <summary>
    /// 报警的最小值阈值。
    /// </summary>
    public double AlarmMin { get; set; }

    /// <summary>
    /// 数据转换规则或表达式。
    /// </summary>
    public string Converstion { get; set; } = String.Empty;

    /// <summary>
    /// 数据保存的范围或阈值。
    /// </summary>
    public double SaveRange { get; set; }
    /// <summary>
    /// 变量数据最后更新时间。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime CreateTime { get; set; } 

    /// <summary>
    /// 变量数据最后更新时间。
    /// </summary>
    public DateTime UpdateTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 最后更新变量数据的用户。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DbUser? UpdateUser { get; set; }

    /// <summary>
    /// 关联的变量表ID。
    /// </summary>
    public int VariableTableId { get; set; }

    /// <summary>
    /// 关联的变量表实体。
    /// </summary>
    [Navigate(NavigateType.ManyToOne, nameof(VariableTableId))]
    public DbVariableTable? VariableTable { get; set; }

    /// <summary>
    /// 关联的MQTT配置列表。
    /// </summary>
    [Navigate(typeof(DbVariableDataMqtt), "VariableDataId", "MqttId")]
    public List<DbMqtt>? Mqtts { get; set; }
}