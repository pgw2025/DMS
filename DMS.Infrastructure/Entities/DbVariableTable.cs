using DMS.Core.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 表示数据库中的变量表实体。
/// </summary>
[SugarTable("VariableTable")]
public class DbVariableTable
{
    /// <summary>
    /// 变量表中包含的数据变量列表。
    /// </summary>
    [Navigate(NavigateType.OneToMany, nameof(DbVariable.VariableTableId))]
    public List<DbVariable>? Variables { get; set; }

    /// <summary>
    /// 变量表关联的设备。
    /// </summary>
    [Navigate(NavigateType.ManyToOne, nameof(DeviceId))]
    public DbDevice? Device { get; set; }

    /// <summary>
    /// 变量表关联的设备ID。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public int? DeviceId { get; set; }

    /// <summary>
    /// 变量表描述。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 变量表的唯一标识符。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增 
    public int Id { get; set; }

    /// <summary>
    /// 表示变量表是否处于活动状态。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 变量表名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 变量表使用的协议类型。
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public ProtocolType ProtocolType { get; set; }
}