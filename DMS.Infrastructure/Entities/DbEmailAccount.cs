using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace DMS.Infrastructure.Entities
{
    /// <summary>
    /// 邮件账户数据库实体
    /// </summary>
    [SugarTable("email_accounts")]
    public class DbEmailAccount
    {
        /// <summary>
        /// 账户ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 账户名称
        /// </summary>
        [SugarColumn(Length = 100)]
        public string Name { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        [SugarColumn(Length = 255)]
        public string EmailAddress { get; set; }

        /// <summary>
        /// SMTP服务器地址
        /// </summary>
        [SugarColumn(Length = 100)]
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
        [SugarColumn(Length = 100)]
        public string Username { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [SugarColumn(Length = 100)]
        public string Password { get; set; }

        /// <summary>
        /// IMAP服务器地址
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true)]
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