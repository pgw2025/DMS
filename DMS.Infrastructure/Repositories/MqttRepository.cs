using System.Diagnostics;
using AutoMapper;
using DMS.Infrastructure.Entities;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Interfaces;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// Mqtt仓储类，用于操作DbMqtt实体
/// </summary>
public class MqttRepository : BaseRepository<DbMqtt>
{

    public MqttRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <summary>
    /// 根据ID获取Mqtt配置
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public override async Task<DbMqtt> GetByIdAsync(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Queryable<DbMqtt>()
                             .In(id)
                             .SingleAsync();
        stopwatch.Stop();
        NlogHelper.Info($"根据ID '{id}' 获取Mqtt配置耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 获取所有Mqtt配置
    /// </summary>
    /// <returns></returns>
    public override async Task<List<DbMqtt>> GetAllAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Queryable<DbMqtt>()
                             .Includes(m => m.VariableMqtts, vm => vm.Variable)
                             .Includes(m => m.VariableMqtts, vm => vm.Mqtt)
                             .ToListAsync();
        stopwatch.Stop();
        NlogHelper.Info($"获取所有Mqtt配置耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }


}