using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// 邮件模板仓储接口
    /// </summary>
    public interface IEmailTemplateRepository : IBaseRepository<EmailTemplate>
    {
        /// <summary>
        /// 根据代码获取邮件模板
        /// </summary>
        Task<EmailTemplate> GetByCodeAsync(string code);

        /// <summary>
        /// 获取所有启用的邮件模板
        /// </summary>
        Task<List<EmailTemplate>> GetActiveTemplatesAsync();
    }
}