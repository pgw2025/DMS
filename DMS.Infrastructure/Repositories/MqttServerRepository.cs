using System.Diagnostics;
using AutoMapper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using Microsoft.Extensions.Logging;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// MQTT服务器仓储实现类，负责MQTT服务器数据的持久化操作。
/// 继承自 <see cref="BaseRepository{DbMqttServer}"/> 并实现 <see cref="IMqttServerRepository"/> 接口。
/// </summary>
public class MqttServerRepository : BaseRepository<DbMqttServer>, IMqttServerRepository
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    /// <param name="logger">日志记录器实例。</param>
    public MqttServerRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<MqttServerRepository> logger)
        : base(dbContext, logger)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取单个MQTT服务器。
    /// </summary>
    /// <param name="id">MQTT服务器的唯一标识符。</param>
    /// <returns>对应的MQTT服务器实体，如果不存在则为null。</returns>
    public async Task<MqttServer> GetByIdAsync(int id)
    {
        var dbMqttServer = await base.GetByIdAsync(id);
        return _mapper.Map<MqttServer>(dbMqttServer);
    }

    /// <summary>
    /// 异步获取所有MQTT服务器。
    /// </summary>
    /// <returns>包含所有MQTT服务器实体的列表。</returns>
    public async Task<List<MqttServer>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<MqttServer>>(dbList);
    }

    /// <summary>
    /// 异步添加新MQTT服务器。
    /// </summary>
    /// <param name="entity">要添加的MQTT服务器实体。</param>
    /// <returns>添加成功后的MQTT服务器实体（包含数据库生成的ID等信息）。</returns>
    public async Task<MqttServer> AddAsync(MqttServer entity)
    {
        var dbMqttServer = await base.AddAsync(_mapper.Map<DbMqttServer>(entity));
        return _mapper.Map(dbMqttServer, entity);
    }

    /// <summary>
    /// 异步更新现有MQTT服务器。
    /// </summary>
    /// <param name="entity">要更新的MQTT服务器实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAsync(MqttServer entity) => await base.UpdateAsync(_mapper.Map<DbMqttServer>(entity));

    /// <summary>
    /// 异步删除MQTT服务器。
    /// </summary>
    /// <param name="entity">要删除的MQTT服务器实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(MqttServer entity) => await base.DeleteAsync(_mapper.Map<DbMqttServer>(entity));
    
    /// <summary>
    /// 异步根据ID删除MQTT服务器。
    /// </summary>
    /// <param name="id">要删除MQTT服务器的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new DbMqttServer() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Delete {typeof(DbMqttServer)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 异步获取指定数量的MQTT服务器。
    /// </summary>
    /// <param name="number">要获取的MQTT服务器数量。</param>
    /// <returns>包含指定数量MQTT服务器实体的列表。</returns>
    public new async Task<List<MqttServer>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<MqttServer>>(dbList);

    }

    public async Task<List<MqttServer>> AddBatchAsync(List<MqttServer> entities)
    {
        var dbEntities = _mapper.Map<List<DbMqttServer>>(entities);
        var addedEntities = await base.AddBatchAsync(dbEntities);
        return _mapper.Map<List<MqttServer>>(addedEntities);
    }
}