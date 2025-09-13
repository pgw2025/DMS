namespace DMS.Application.DTOs
{
    /// <summary>
    /// 创建邮件账户请求DTO
    /// </summary>
    public class CreateEmailAccountRequest
    {
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
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// 是否启用SSL
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// IMAP服务器地址
        /// </summary>
        public string? ImapServer { get; set; }

        /// <summary>
        /// IMAP端口号
        /// </summary>
        public int ImapPort { get; set; } = 993;

        /// <summary>
        /// 是否为默认账户
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; } = true;
    }
}