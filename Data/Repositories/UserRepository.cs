using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using PMSWPF.Data.Entities;

namespace PMSWPF.Data.Repositories;

/// <summary>
/// 用户仓储类，用于操作DbUser实体
/// </summary>
public class UserRepository
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 根据ID获取用户
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<DbUser> GetByIdAsync(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Queryable<DbUser>().In(id).SingleAsync();
            stopwatch.Stop();
            Logger.Info($"根据ID '{id}' 获取用户耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 获取所有用户
    /// </summary>
    /// <returns></returns>
    public async Task<List<DbUser>> GetAllAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Queryable<DbUser>().ToListAsync();
            stopwatch.Stop();
            Logger.Info($"获取所有用户耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 新增用户
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns></returns>
    public async Task<int> AddAsync(DbUser user)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Insertable(user).ExecuteReturnIdentityAsync();
            stopwatch.Stop();
            Logger.Info($"新增用户 '{user.Id}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 更新用户
    /// </summary>
    /// <param name="user">用户实体</param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(DbUser user)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Updateable(user).ExecuteCommandAsync();
            stopwatch.Stop();
            Logger.Info($"更新用户 '{user.Id}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 根据ID删除用户
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Deleteable<DbUser>().In(id).ExecuteCommandAsync();
            stopwatch.Stop();
            Logger.Info($"删除用户ID '{id}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
}