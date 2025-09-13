using System.ComponentModel.DataAnnotations;

namespace DMS.Core.Models
{
    /// <summary>
    /// 邮件账户配置实体
    /// </summary>
    public class EmailAccount
    {
        /// <summary>
        /// 账户ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 账户名称
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string EmailAddress { get; set; }

        /// <summary>
        /// SMTP服务器地址
        /// </summary>
        [Required]
        [MaxLength(100)]
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
        [Required]
        [MaxLength(100)]
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Password { get; set; }

        /// <summary>
        /// IMAP服务器地址
        /// </summary>
        [MaxLength(100)]
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