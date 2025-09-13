using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.Core.Models
{
    /// <summary>
    /// 报警历史记录实体
    /// </summary>
    public class AlarmHistory
    {
        /// <summary>
        /// 主键ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 变量ID
        /// </summary>
        public int VariableId { get; set; }

        /// <summary>
        /// 变量名称
        /// </summary>
        [MaxLength(100)]
        public string VariableName { get; set; }

        /// <summary>
        /// 当前值
        /// </summary>
        public double CurrentValue { get; set; }

        /// <summary>
        /// 阈值
        /// </summary>
        public double ThresholdValue { get; set; }

        /// <summary>
        /// 报警类型 (High, Low, Deadband, BooleanChange)
        /// </summary>
        [MaxLength(50)]
        public string AlarmType { get; set; }

        /// <summary>
        /// 报警消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 报警触发时间
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 是否已确认
        /// </summary>
        public bool IsAcknowledged { get; set; }

        /// <summary>
        /// 确认时间
        /// </summary>
        public DateTime? AcknowledgedAt { get; set; }

        /// <summary>
        /// 确认人
        /// </summary>
        [MaxLength(100)]
        public string AcknowledgedBy { get; set; }
    }
}