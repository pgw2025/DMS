using System;
using SqlSugar;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 表示数据库中的变量数据历史实体。
/// </summary>
[SugarTable("VarDataHistory")]
public class DbVariableHistory
{
    /// <summary>
    /// 历史记录唯一标识符。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 变量名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 节点ID，用于标识变量在设备或系统中的唯一路径。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string NodeId { get; set; } = String.Empty;
    
    /// <summary>
    /// 变量当前原始数据值。
    /// </summary>
    public string DataValue { get; set; } = String.Empty;

    /// <summary>
    /// 关联的DbVariableData的ID。
    /// </summary>
    public int VariableId { get; set; }

    /// <summary>
    /// 关联的DbVariableData实体。
    /// </summary>
    [Navigate(NavigateType.ManyToOne, nameof(VariableId))]
    public DbVariable? Variable { get; set; }

    /// <summary>
    /// 历史记录的时间戳。
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}