using System;
using System.Collections.Generic;
using DMS.Core.Models.Triggers;
using SqlSugar;
using SqlSugar.DbConvert;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 触发器定义实体类，对应数据库中的 TriggerDefinitions 表。
/// </summary>
[SugarTable("Triggers")]
public class DbTrigger
{
    /// <summary>
    /// 触发器唯一标识符，主键。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 触发器名称
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = true)]
    public string Name { get; set; } = "";

    /// <summary>
    /// 触发器是否处于激活状态。
    /// </summary>
    public bool IsActive { get; set; } = true;

    // --- 动作部分 ---

    /// <summary>
    /// 动作类型。
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public ActionType Action { get; set; }

    /// <summary>
    /// 动作配置 JSON 字符串，存储特定于动作类型的配置（如邮件收件人列表、模板 ID 等）。
    /// </summary>
    [SugarColumn(Length = 2000, IsNullable = true)]
    public string ActionConfigurationJson { get; set; } = "{}";

    // --- 抑制与状态部分 ---

    /// <summary>
    /// 抑制持续时间。如果设置了此值，在触发一次后，在该时间段内不会再触发。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public long? SuppressionDurationTicks { get; set; }

    /// <summary>
    /// 上次触发的时间。用于抑制逻辑。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// 触发器描述。
    /// </summary>
    [SugarColumn(Length = 500, IsNullable = true)]
    public string Description { get; set; } = "";

    /// <summary>
    /// 创建时间。
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最后更新时间。
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// 关联的变量 ID 列表（通过中间表关联）。
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<int> VariableIds { get; set; } = new List<int>();
}