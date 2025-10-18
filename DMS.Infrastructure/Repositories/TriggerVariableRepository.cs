using System.Diagnostics;
using DMS.Core.Interfaces.Repositories;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using AutoMapper;
using Microsoft.Extensions.Logging;
using DMS.Core.Models.Triggers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 触发器与变量关联仓储实现类，负责触发器与变量关联数据的持久化操作。
/// 继承自 <see cref="BaseRepository{DbTriggerVariable}"/> 并实现 <see cref="ITriggerVariableRepository"/> 接口。
/// </summary>
public class TriggerVariableRepository : BaseRepository<DbTriggerVariable>, ITriggerVariableRepository
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    /// <param name="logger">日志记录器实例。</param>
    public TriggerVariableRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<TriggerVariableRepository> logger)
        : base(dbContext, logger)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取单个触发器与变量关联。
    /// </summary>
    /// <param name="id">触发器与变量关联的唯一标识符。</param>
    /// <returns>对应的触发器与变量关联实体，如果不存在则为null。</returns>
    public async Task<TriggerVariable> GetByIdAsync(int id)
    {
        var dbTriggerVariable = await base.GetByIdAsync(id);
        return _mapper.Map<TriggerVariable>(dbTriggerVariable);
    }

    /// <summary>
    /// 异步获取所有触发器与变量关联。
    /// </summary>
    /// <returns>包含所有触发器与变量关联实体的列表。</returns>
    public async Task<List<TriggerVariable>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<TriggerVariable>>(dbList);
    }

    /// <summary>
    /// 异步添加新触发器与变量关联。
    /// </summary>
    /// <param name="entity">要添加的触发器与变量关联实体。</param>
    /// <returns>添加成功后的触发器与变量关联实体（包含数据库生成的ID等信息）。</returns>
    public async Task<TriggerVariable> AddAsync(TriggerVariable entity)
    {
        var dbTriggerVariable = await base.AddAsync(_mapper.Map<DbTriggerVariable>(entity));
        return _mapper.Map(dbTriggerVariable, entity);
    }

    /// <summary>
    /// 异步更新现有触发器与变量关联。
    /// </summary>
    /// <param name="entity">要更新的触发器与变量关联实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAsync(TriggerVariable entity) => await base.UpdateAsync(_mapper.Map<DbTriggerVariable>(entity));

    /// <summary>
    /// 异步删除触发器与变量关联。
    /// </summary>
    /// <param name="entity">要删除的触发器与变量关联实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(TriggerVariable entity) => await base.DeleteAsync(_mapper.Map<DbTriggerVariable>(entity));
    
    /// <summary>
    /// 异步根据ID删除触发器与变量关联。
    /// </summary>
    /// <param name="id">要删除触发器与变量关联的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Deleteable(new DbTriggerVariable() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Delete {typeof(DbTriggerVariable)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 异步获取指定数量的触发器与变量关联。
    /// </summary>
    /// <param name="number">要获取的触发器与变量关联数量。</param>
    /// <returns>包含指定数量触发器与变量关联实体的列表。</returns>
    public new async Task<List<TriggerVariable>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<TriggerVariable>>(dbList);
    }

    public async Task<List<TriggerVariable>> AddBatchAsync(List<TriggerVariable> entities)
    {
        var dbEntities = _mapper.Map<List<DbTriggerVariable>>(entities);
        var addedEntities = await base.AddBatchAsync(dbEntities);
        return _mapper.Map<List<TriggerVariable>>(addedEntities);
    }
}