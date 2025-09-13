using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace DMS.Infrastructure.Entities
{
    /// <summary>
    /// 邮件消息数据库实体
    /// </summary>
    [SugarTable("email_messages")]
    public class DbEmailMessage
    {
        /// <summary>
        /// 邮件ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 关联的邮件账户ID
        /// </summary>
        public int EmailAccountId { get; set; }

        /// <summary>
        /// 发件人邮箱地址
        /// </summary>
        [SugarColumn(Length = 255)]
        public string From { get; set; }

        /// <summary>
        /// 收件人邮箱地址（多个用分号分隔）
        /// </summary>
        [SugarColumn(Length = 1000)]
        public string To { get; set; }

        /// <summary>
        /// 抄送邮箱地址（多个用分号分隔）
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true)]
        public string? Cc { get; set; }

        /// <summary>
        /// 密送邮箱地址（多个用分号分隔）
        /// </summary>
        [SugarColumn(Length = 1000, IsNullable = true)]
        public string? Bcc { get; set; }

        /// <summary>
        /// 邮件主题
        /// </summary>
        [SugarColumn(Length = 500)]
        public string Subject { get; set; }

        /// <summary>
        /// 邮件正文
        /// </summary>
        [SugarColumn(Length = 4000)]
        public string Body { get; set; }

        /// <summary>
        /// 是否为HTML格式
        /// </summary>
        public bool IsHtml { get; set; } = true;

        /// <summary>
        /// 邮件发送状态
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Status { get; set; } = "Pending";

        /// <summary>
        /// 发送时间
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}