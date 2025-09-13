namespace DMS.Application.DTOs
{
    /// <summary>
    /// 邮件日志DTO
    /// </summary>
    public class EmailLogDto
    {
        /// <summary>
        /// 日志ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 关联的邮件ID
        /// </summary>
        public int EmailMessageId { get; set; }

        /// <summary>
        /// 日志级别
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// 日志消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 异常详情
        /// </summary>
        public string? Exception { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}