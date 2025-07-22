using DMS.Core.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 变量表
/// </summary>
public class DbVariableTable
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    /// <summary>
    /// 变量表名称
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string Description { get; set; }
    /// <summary>
    /// 是否激活
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// 设备ID
    /// </summary>
    public int DeviceId { get; set; }
    /// <summary>
    /// 关联的设备
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public DbDevice Device { get; set; }
    
    /// <summary>
    /// 协议类型
    /// </summary>
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public ProtocolType Protocol { get; set; } // 对应 ProtocolType 枚举
    
    /// <summary>
    /// 此设备包含的变量表集合。
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<DbVariable> Variables { get; set; } = new();
}