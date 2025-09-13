using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// 邮件消息仓储接口
    /// </summary>
    public interface IEmailMessageRepository : IBaseRepository<EmailMessage>
    {
        /// <summary>
        /// 根据状态获取邮件消息
        /// </summary>
        Task<List<EmailMessage>> GetByStatusAsync(EmailSendStatus status);

        /// <summary>
        /// 获取指定时间范围内的邮件消息
        /// </summary>
        Task<List<EmailMessage>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}