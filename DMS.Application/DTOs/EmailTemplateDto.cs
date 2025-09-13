namespace DMS.Application.DTOs
{
    /// <summary>
    /// 邮件模板DTO
    /// </summary>
    public class EmailTemplateDto
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 模板代码（唯一标识）
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 模板主题
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// 模板内容
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 是否为HTML格式
        /// </summary>
        public bool IsHtml { get; set; }

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