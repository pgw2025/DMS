using System;
using System.Collections.Generic;

namespace DMS.Core.Models.Triggers
{
    /// <summary>
    /// 触发器条件类型枚举
    /// </summary>
    public enum ConditionType
    {
        GreaterThan,
        LessThan,
        EqualTo,
        NotEqualTo,
        InRange, // 值在 LowerBound 和 UpperBound 之间 (包含边界)
        OutOfRange // 值低于 LowerBound 或高于 UpperBound
    }

    /// <summary>
    /// 触发器动作类型枚举
    /// </summary>
    public enum ActionType
    {
        SendEmail,
        ActivateAlarm,
        WriteToLog,
        // 未来可扩展: ExecuteScript, CallApi, etc.
    }

    /// <summary>
    /// 触发器定义领域模型
    /// </summary>
    public class TriggerDefinition
    {
        /// <summary>
        /// 触发器唯一标识符
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联的变量列表
        /// </summary>
        public List<Variable> Variables { get; set; } = new List<Variable>();

        /// <summary>
        /// 触发器是否处于激活状态
        /// </summary>
        public bool IsActive { get; set; } = true;

        // --- 条件部分 ---

        /// <summary>
        /// 触发条件类型
        /// </summary>
        public ConditionType Condition { get; set; }

        /// <summary>
        /// 阈值 (用于 GreaterThan, LessThan, EqualTo, NotEqualTo)
        /// </summary>
        public double? Threshold { get; set; }

        /// <summary>
        /// 下限 (用于 InRange, OutOfRange)
        /// </summary>
        public double? LowerBound { get; set; }

        /// <summary>
        /// 上限 (用于 InRange, OutOfRange)
        /// </summary>
        public double? UpperBound { get; set; }

        // --- 动作部分 ---

        /// <summary>
        /// 动作类型
        /// </summary>
        public ActionType Action { get; set; }

        /// <summary>
        /// 动作配置 JSON 字符串，存储特定于动作类型的配置（如邮件收件人列表、模板 ID 等）
        /// </summary>
        public string ActionConfigurationJson { get; set; } = "{}";

        // --- 抑制与状态部分 ---

        /// <summary>
        /// 抑制持续时间。如果设置了此值，在触发一次后，在该时间段内不会再触发。
        /// </summary>
        public TimeSpan? SuppressionDuration { get; set; }

        /// <summary>
        /// 上次触发的时间。用于抑制逻辑。
        /// </summary>
        public DateTime? LastTriggeredAt { get; set; }

        /// <summary>
        /// 触发器描述
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}