using PMSWPF.Enums;
using PMSWPF.Models;
using SqlSugar;
using SqlSugar.DbConvert;

namespace PMSWPF.Data.Entities;

[SugarTable("VariableTable")]
public class DbVariableTable
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增 
    public int Id { get; set; }

    public string Name { get; set; }

    [SugarColumn(IsNullable = true)] public string? Description { get; set; }

    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public ProtocolType ProtocolType { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(DbDataVariable.VariableTableId))]
    public List<DbDataVariable>? DataVariables { get; set; }

    [SugarColumn(IsNullable = true)] public int? DeviceId { get; set; }

    [Navigate(NavigateType.ManyToOne, nameof(DeviceId))]
    public Device? Device { get; set; }
}