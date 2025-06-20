using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace PMSWPF.Data.Entities;
[SugarTable("DataVariable")]
public class DbDataVariable
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]//数据库是自增才配自增 
    public int  Id { get; set; }
    [SugarColumn(IsNullable = true)]
    public int ?VariableTableId { get; set; }
    [Navigate(NavigateType.ManyToOne, nameof(DbDataVariable.VariableTableId))]
    public DbVariableTable? VariableTable { get; set; }
    public string Name { get; set; }
    [SugarColumn(IsNullable = true)]
    public string? Description { get; set; }
    public string NodeId { get; set; }
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public ProtocolType ProtocolType { get; set; }
    public string DataType { get; set; }
    [SugarColumn(IsNullable = true)]
    public List<DbMqtt>? Mqtts { get; set; }
    public string DataValue { get; set; }
    public string DisplayValue { get; set; }
    public DateTime UpdateTime { get; set; }
    [SugarColumn(IsNullable = true)]
    public DbUser? UpdateUser { get; set; }
    public string Converstion { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsActive { get; set; }  
    public bool IsSave { get; set; }
    public Double SaveRange  { get; set; }
    public bool IsAlarm { get; set; }
    public Double AlarmMin { get; set; }
    public Double AlarmMax { get; set; }
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public SignalType SignalType { get; set; }
    
}