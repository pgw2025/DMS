using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace DMS.Infrastructure.Entities
{
    /// <summary>
    /// 邮件日志数据库实体
    /// </summary>
    [SugarTable("email_logs")]
    public class DbEmailLog
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 关联的邮件ID
        /// </summary>
        public int EmailMessageId { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        [SugarColumn(Length = 20)]
        public string Level { get; set; }

        /// <summary>
        /// 日志消息
        /// </summary>
        [SugarColumn(Length = 4000)]
        public string Message { get; set; }

        /// <summary>
        /// 异常详情
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true)]
        public string? Exception { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}