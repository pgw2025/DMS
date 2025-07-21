using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IUserRepository : IBaseRepository<User>
{
    /// <summary>
    /// 异步根据用户名获取用户。
    /// </summary>
    /// <param name="username">用户名。</param>
    /// <returns>用户对象，如果不存在则为null。</returns>
    Task<User> GetByUsernameAsync(string username);
}