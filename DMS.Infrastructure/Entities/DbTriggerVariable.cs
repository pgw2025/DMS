using SqlSugar;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 数据库实体：表示触发器与变量的多对多关联关系。
/// </summary>
[SugarTable("TriggerVariables")]
public class DbTriggerVariable
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 外键，指向 TriggerDefinitions 表的 Id。
    /// </summary>
    public int TriggerDefinitionId { get; set; }

    /// <summary>
    /// 外键，指向 Variables 表的 Id。
    /// </summary>
    public int VariableId { get; set; }
}