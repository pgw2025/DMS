namespace DMS.Application.DTOs
{
    /// <summary>
    /// 邮件消息DTO
    /// </summary>
    public class EmailMessageDto
    {
        /// <summary>
        /// 邮件ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联的邮件账户ID
        /// </summary>
        public int EmailAccountId { get; set; }

        /// <summary>
        /// 发件人邮箱地址
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// 收件人邮箱地址（多个用分号分隔）
        /// </summary>
        public string To { get; set; }

        /// <summary>
        /// 抄送邮箱地址（多个用分号分隔）
        /// </summary>
        public string? Cc { get; set; }

        /// <summary>
        /// 密送邮箱地址（多个用分号分隔）
        /// </summary>
        public string? Bcc { get; set; }

        /// <summary>
        /// 邮件主题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 邮件正文
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 是否为HTML格式
        /// </summary>
        public bool IsHtml { get; set; }

        /// <summary>
        /// 邮件发送状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}