using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;
using ProtocolType = PMSWPF.Enums.ProtocolType;

namespace PMSWPF.Data.Entities;

[SugarTable("Device")]
public class DbDevice
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增 
    public int Id { get; set; }

    public string Name { get; set; }

    [SugarColumn(IsNullable = true)] public string? Description { get; set; }

    public string Ip { get; set; }
    public bool IsActive { get; set; }
    public bool IsRuning { get; set; }

    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public DeviceType DeviceType { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(DbVariableTable.DeviceId))]
    [SugarColumn(IsNullable = true)]
    public List<DbVariableTable>? VariableTables { get; set; }

    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public ProtocolType ProtocolType { get; set; }
}