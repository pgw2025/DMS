using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories
{
    /// <summary>
    /// 邮件账户仓储接口
    /// </summary>
    public interface IEmailAccountRepository : IBaseRepository<EmailAccount>
    {
        /// <summary>
        /// 获取默认邮件账户
        /// </summary>
        Task<EmailAccount> GetDefaultAccountAsync();

        /// <summary>
        /// 获取所有启用的邮件账户
        /// </summary>
        Task<List<EmailAccount>> GetActiveAccountsAsync();
    }
}