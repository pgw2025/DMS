namespace DMS.Application.DTOs
{
    /// <summary>
    /// 邮件账户DTO
    /// </summary>
    public class EmailAccountDto
    {
        /// <summary>
        /// 账户ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 账户名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string EmailAddress { get; set; }

        /// <summary>
        /// SMTP服务器地址
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// SMTP端口号
        /// </summary>
        public int SmtpPort { get; set; }

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        public bool EnableSsl { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// IMAP服务器地址
        /// </summary>
        public string? ImapServer { get; set; }

        /// <summary>
        /// IMAP端口号
        /// </summary>
        public int ImapPort { get; set; }

        /// <summary>
        /// 是否为默认账户
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }

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