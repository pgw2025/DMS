using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace DMS.Infrastructure.Entities
{
    /// <summary>
    /// 邮件模板数据库实体
    /// </summary>
    [SugarTable("email_templates")]
    public class DbEmailTemplate
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int Id { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        [SugarColumn(Length = 100)]
        public string Name { get; set; }

        /// <summary>
        /// 模板代码（唯一标识）
        /// </summary>
        [SugarColumn(Length = 50)]
        public string Code { get; set; }

        /// <summary>
        /// 模板主题
        /// </summary>
        [SugarColumn(Length = 500)]
        public string Subject { get; set; }

        /// <summary>
        /// 模板内容
        /// </summary>
        [SugarColumn(Length = 4000)]
        public string Body { get; set; }

        /// <summary>
        /// 是否为HTML格式
        /// </summary>
        public bool IsHtml { get; set; } = true;

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