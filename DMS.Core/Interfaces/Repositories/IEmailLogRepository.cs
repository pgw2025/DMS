using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// 邮件日志仓储接口
    /// </summary>
    public interface IEmailLogRepository : IBaseRepository<EmailLog>
    {
        /// <summary>
        /// 根据邮件消息ID获取日志
        /// </summary>
        Task<List<EmailLog>> GetByEmailMessageIdAsync(int emailMessageId);

        /// <summary>
        /// 根据日期范围获取日志
        /// </summary>
        Task<List<EmailLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    }
}