using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace PMSWPF.Data.Entities;

[SugarTable("VariableData")]
public class DbVariableData
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增 
    public int Id { get; set; }

    [SugarColumn(IsNullable = true)] public int? VariableTableId { get; set; }

    [Navigate(NavigateType.ManyToOne, nameof(VariableTableId))]
    public DbVariableTable? VariableTable { get; set; }

    public string Name { get; set; }

    [SugarColumn(IsNullable = true)] public string? Description { get; set; }

    public string NodeId { get; set; }

    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public ProtocolType ProtocolType { get; set; }

    public string DataType { get; set; }

    [SugarColumn(IsNullable = true)] public List<DbMqtt>? Mqtts { get; set; }

    public string DataValue { get; set; } = String.Empty;
    public string DisplayValue { get; set; } = String.Empty;
    public DateTime UpdateTime { get; set; }

    [SugarColumn(IsNullable = true)] public DbUser? UpdateUser { get; set; }

    public string Converstion { get; set; } = String.Empty;
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }
    public bool IsSave { get; set; }
    public double SaveRange { get; set; }
    public bool IsAlarm { get; set; }
    public double AlarmMin { get; set; }
    public double AlarmMax { get; set; }

    [SugarColumn(ColumnDataType = "varchar(20)", IsNullable = true, SqlParameterDbType = typeof(EnumToStringConvert))]
    public SignalType SignalType { get; set; }
}