namespace DMS.Application.DTOs
{
    /// <summary>
    /// 发送邮件请求DTO
    /// </summary>
    public class SendEmailRequest
    {
        /// <summary>
        /// 关联的邮件账户ID
        /// </summary>
        public int EmailAccountId { get; set; }

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
        public bool IsHtml { get; set; } = true;
    }
}