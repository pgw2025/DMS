using System.Collections.Generic;
using System.Threading.Tasks;
using PMSWPF.Data.Entities;

namespace PMSWPF.Data.Repositories;

/// <summary>
/// Mqtt仓储类，用于操作DbMqtt实体
/// </summary>
public class MqttRepository
{
    /// <summary>
    /// 根据ID获取Mqtt配置
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<DbMqtt> GetByIdAsync(int id)
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Queryable<DbMqtt>().In(id).SingleAsync();
        }
    }

    /// <summary>
    /// 获取所有Mqtt配置
    /// </summary>
    /// <returns></returns>
    public async Task<List<DbMqtt>> GetAllAsync()
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Queryable<DbMqtt>().ToListAsync();
        }
    }

    /// <summary>
    /// 新增Mqtt配置
    /// </summary>
    /// <param name="mqtt">Mqtt实体</param>
    /// <returns></returns>
    public async Task<int> AddAsync(DbMqtt mqtt)
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Insertable(mqtt).ExecuteReturnIdentityAsync();
        }
    }

    /// <summary>
    /// 更新Mqtt配置
    /// </summary>
    /// <param name="mqtt">Mqtt实体</param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(DbMqtt mqtt)
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Updateable(mqtt).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 根据ID删除Mqtt配置
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(int id)
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Deleteable<DbMqtt>().In(id).ExecuteCommandAsync();
        }
    }
}