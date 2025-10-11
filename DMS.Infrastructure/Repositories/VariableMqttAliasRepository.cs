using System.Diagnostics;
using DMS.Core.Interfaces.Repositories;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;
using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 变量与MQTT别名关联仓储实现类，负责变量与MQTT别名关联数据的持久化操作。
/// 继承自 <see cref="BaseRepository{DbVariableMqttAlias}"/> 并实现 <see cref="IVariableMqttAliasRepository"/> 接口。
/// </summary>
public class VariableMqttAliasRepository : BaseRepository<DbVariableMqttAlias>, IVariableMqttAliasRepository
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    /// <param name="logger">日志记录器实例。</param>
    public VariableMqttAliasRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<VariableMqttAliasRepository> logger)
        : base(dbContext, logger)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取单个变量与MQTT别名关联。
    /// </summary>
    /// <param name="id">变量与MQTT别名关联的唯一标识符。</param>
    /// <returns>对应的变量与MQTT别名关联实体，如果不存在则为null。</returns>
    public async Task<MqttAlias> GetByIdAsync(int id)
    {
        var dbVariableMqttAlias = await base.GetByIdAsync(id);
        return _mapper.Map<MqttAlias>(dbVariableMqttAlias);
    }

    /// <summary>
    /// 异步获取所有变量与MQTT别名关联。
    /// </summary>
    /// <returns>包含所有变量与MQTT别名关联实体的列表。</returns>
    public async Task<List<MqttAlias>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<MqttAlias>>(dbList);
    }

    /// <summary>
    /// 异步添加新变量与MQTT别名关联。
    /// </summary>
    /// <param name="entity">要添加的变量与MQTT别名关联实体。</param>
    /// <returns>添加成功后的变量与MQTT别名关联实体（包含数据库生成的ID等信息）。</returns>
    public async Task<MqttAlias> AddAsync(MqttAlias entity)
    {
        var dbVariableMqttAlias = await base.AddAsync(_mapper.Map<DbVariableMqttAlias>(entity));
        return _mapper.Map(dbVariableMqttAlias, entity);
    }

    /// <summary>
    /// 异步更新现有变量与MQTT别名关联。
    /// </summary>
    /// <param name="entity">要更新的变量与MQTT别名关联实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAsync(MqttAlias entity) => await base.UpdateAsync(_mapper.Map<DbVariableMqttAlias>(entity));

    /// <summary>
    /// 异步删除变量与MQTT别名关联。
    /// </summary>
    /// <param name="entity">要删除的变量与MQTT别名关联实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(MqttAlias entity) => await base.DeleteAsync(_mapper.Map<DbVariableMqttAlias>(entity));
    
    /// <summary>
    /// 异步根据ID删除变量与MQTT别名关联。
    /// </summary>
    /// <param name="id">要删除变量与MQTT别名关联的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Deleteable(new DbVariableMqttAlias() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Delete {typeof(DbVariableMqttAlias)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 异步获取指定数量的变量与MQTT别名关联。
    /// </summary>
    /// <param name="number">要获取的变量与MQTT别名关联数量。</param>
    /// <returns>包含指定数量变量与MQTT别名关联实体的列表。</returns>
    public new async Task<List<MqttAlias>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<MqttAlias>>(dbList);

    }

    public async Task<List<MqttAlias>> AddBatchAsync(List<MqttAlias> entities)
    {
        var dbEntities = _mapper.Map<List<DbVariableMqttAlias>>(entities);
        var addedEntities = await base.AddBatchAsync(dbEntities);
        return _mapper.Map<List<MqttAlias>>(addedEntities);
    }
}