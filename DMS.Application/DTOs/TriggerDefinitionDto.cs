using DMS.Core.Models.Triggers;

// 引入枚举

namespace DMS.Application.DTOs
{
    /// <summary>
    /// 触发器定义 DTO (用于应用层与表示层之间的数据传输)
    /// </summary>
    public class TriggerDefinitionDto
    {
        /// <summary>
        /// 触发器唯一标识符
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联的变量 ID
        /// </summary>
        public int VariableId { get; set; }

        /// <summary>
        /// 触发器是否处于激活状态
        /// </summary>
        public bool IsActive { get; set; }

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
        /// 动作配置 JSON 字符串
        /// </summary>
        public string ActionConfigurationJson { get; set; }

        // --- 抑制与状态部分 ---

        /// <summary>
        /// 抑制持续时间
        /// </summary>
        public TimeSpan? SuppressionDuration { get; set; }

        /// <summary>
        /// 上次触发的时间
        /// </summary>
        public DateTime? LastTriggeredAt { get; set; }

        /// <summary>
        /// 触发器描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}