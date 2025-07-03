using System.Collections.Generic;
using System.Threading.Tasks;
using PMSWPF.Data.Entities;

namespace PMSWPF.Data.Repositories;

/// <summary>
/// 用户仓储类，用于操作DbUser实体
/// </summary>
public class UserRepository
{
    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<DbUser> GetByIdAsync(int id)
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Queryable<DbUser>().In(id).SingleAsync();
        }
    }

    /// <summary>
    /// 获取所有用户
    /// </summary>
    /// <returns></returns>
    public async Task<List<DbUser>> GetAllAsync()
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Queryable<DbUser>().ToListAsync();
        }
    }

    /// <summary>
    /// 新增用户
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns></returns>
    public async Task<int> AddAsync(DbUser user)
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Insertable(user).ExecuteReturnIdentityAsync();
        }
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(DbUser user)
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Updateable(user).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 根据ID删除用户
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(int id)
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Deleteable<DbUser>().In(id).ExecuteCommandAsync();
        }
    }
}