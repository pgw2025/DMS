using System.ComponentModel.DataAnnotations;

namespace DMS.Core.Models
{
    /// <summary>
    /// 邮件日志实体
    /// </summary>
    public class EmailLog
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联的邮件ID
        /// </summary>
        public int EmailMessageId { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string Level { get; set; }

        /// <summary>
        /// 日志消息
        /// </summary>
        [Required]
        public string Message { get; set; }

        /// <summary>
        /// 异常详情
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}