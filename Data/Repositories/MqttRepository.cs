using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using PMSWPF.Data.Entities;
using PMSWPF.Models;
using PMSWPF.Extensions;

namespace PMSWPF.Data.Repositories;

/// <summary>
/// Mqtt仓储类，用于操作DbMqtt实体
/// </summary>
public class MqttRepository
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 根据ID获取Mqtt配置
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<Mqtt> GetById(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Queryable<DbMqtt>().In(id).SingleAsync();
            stopwatch.Stop();
            Logger.Info($"根据ID '{id}' 获取Mqtt配置耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result.CopyTo<Mqtt>();
        }
    }

    /// <summary>
    /// 获取所有Mqtt配置
    /// </summary>
    /// <returns></returns>
    public async Task<List<Mqtt>> GetAll()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Queryable<DbMqtt>().ToListAsync();
            stopwatch.Stop();
            Logger.Info($"获取所有Mqtt配置耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result.CopyTo<List<Mqtt>>();
        }
    }

    /// <summary>
    /// 新增Mqtt配置
    /// </summary>
    /// <param name="mqtt">Mqtt实体</param>
    /// <returns></returns>
    public async Task<int> Add(Mqtt mqtt)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Insertable(mqtt.CopyTo<DbMqtt>()).ExecuteReturnIdentityAsync();
            stopwatch.Stop();
            Logger.Info($"新增Mqtt配置 '{mqtt.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 更新Mqtt配置
    /// </summary>
    /// <param name="mqtt">Mqtt实体</param>
    /// <returns></returns>
    public async Task<int> Edit(Mqtt mqtt)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Updateable(mqtt.CopyTo<DbMqtt>()).ExecuteCommandAsync();
            stopwatch.Stop();
            Logger.Info($"更新Mqtt配置 '{mqtt.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 根据ID删除Mqtt配置
    /// </summary>
    /// <param name="mqtt">Mqtt实体</param>
    /// <returns></returns>
    public async Task<int> Delete(Mqtt mqtt)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Deleteable<DbMqtt>().In(mqtt.Id).ExecuteCommandAsync();
            stopwatch.Stop();
            Logger.Info($"删除Mqtt配置ID '{mqtt.Id}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
}