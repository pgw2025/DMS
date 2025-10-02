using System.Diagnostics;
using AutoMapper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using Microsoft.Extensions.Logging;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 变量仓储实现类，负责变量数据的持久化操作。
/// 继承自 <see cref="BaseRepository{DbVariable}"/> 并实现 <see cref="IVariableRepository"/> 接口。
/// </summary>
public class VariableRepository : BaseRepository<DbVariable>, IVariableRepository
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    /// <param name="logger">日志记录器实例。</param>
    public VariableRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<VariableRepository> logger)
        : base(dbContext, logger)
    {
        _mapper = mapper;
    }


    /// <summary>
    /// 异步根据ID获取单个变量。
    /// </summary>
    /// <param name="id">变量的唯一标识符。</param>
    /// <returns>对应的变量实体，如果不存在则为null。</returns>
    public async Task<Variable> GetByIdAsync(int id)
    {
        var dbVariable = await base.GetByIdAsync(id);
        return _mapper.Map<Variable>(dbVariable);
    }

    /// <summary>
    /// 异步获取所有变量。
    /// </summary>
    /// <returns>包含所有变量实体的列表。</returns>
    public async Task<List<Variable>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<Variable>>(dbList);
    }

    /// <summary>
    /// 异步添加新变量。
    /// </summary>
    /// <param name="entity">要添加的变量实体。</param>
    /// <returns>添加成功后的变量实体（包含数据库生成的ID等信息）。</returns>
    public async Task<Variable> AddAsync(Variable entity)
    {
        var dbVariable = await base.AddAsync(_mapper.Map<DbVariable>(entity));
        return _mapper.Map(dbVariable, entity);
    }

    /// <summary>
    /// 异步更新现有变量。
    /// </summary>
    /// <param name="entity">要更新的变量实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAsync(Variable entity) => await base.UpdateAsync(_mapper.Map<DbVariable>(entity));

    /// <summary>
    /// 异步删除变量。
    /// </summary>
    /// <param name="entity">要删除的变量实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(Variable entity) => await base.DeleteAsync(_mapper.Map<DbVariable>(entity));
    
    
    /// <summary>
    /// 异步根据变量表ID删除变量。
    /// </summary>
    /// <param name="variableTableId">变量表的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByVariableTableIdAsync(int variableTableId)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Deleteable<DbVariable>()
                             .Where(v => v.VariableTableId == variableTableId)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Delete {typeof(DbVariable)} by VariableTableId={variableTableId}, Count={result}, 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 异步根据ID删除变量。
    /// </summary>
    /// <param name="id">要删除变量的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Deleteable(new DbVariable() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Delete {typeof(DbVariable)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 异步根据ID列表批量删除变量。
    /// </summary>
    /// <param name="ids">要删除变量的唯一标识符列表。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdsAsync(List<int> ids)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Deleteable<DbVariable>()
                             .In(ids)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Delete {typeof(DbVariable)},Count={ids.Count},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 异步获取指定数量的变量。
    /// </summary>
    /// <param name="number">要获取的变量数量。</param>
    /// <returns>包含指定数量变量实体的列表。</returns>
    public new async Task<List<Variable>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<Variable>>(dbList);

    }

    public async Task<List<Variable>> AddBatchAsync(List<Variable> entities)
    {
        var dbEntities = _mapper.Map<List<DbVariable>>(entities);
        var addedEntities = await base.AddBatchAsync(dbEntities);
        return _mapper.Map<List<Variable>>(addedEntities);
    }

    /// <summary>
    /// 异步根据OPC UA NodeId获取单个变量实体。
    /// </summary>
    /// <param name="opcUaNodeId">OPC UA NodeId。</param>
    /// <returns>找到的变量实体，如果不存在则返回null。</returns>
    public async Task<Variable?> GetByOpcUaNodeIdAsync(string opcUaNodeId)
    {
        var dbVariable = await _dbContext.GetInstance().Queryable<DbVariable>()
                                 .Where(v => v.OpcUaNodeId == opcUaNodeId)
                                 .FirstAsync();
        return dbVariable == null ? null : _mapper.Map<Variable>(dbVariable);
    }

    /// <summary>
    /// 异步根据OPC UA NodeId列表获取变量实体列表。
    /// </summary>
    /// <param name="opcUaNodeIds">OPC UA NodeId列表。</param>
    /// <returns>找到的变量实体列表。</returns>
    public async Task<List<Variable>> GetByOpcUaNodeIdsAsync(List<string> opcUaNodeIds)
    {
        var dbVariables = await _dbContext.GetInstance().Queryable<DbVariable>()
                                  .Where(v => opcUaNodeIds.Contains(v.OpcUaNodeId))
                                  .ToListAsync();
        return _mapper.Map<List<Variable>>(dbVariables);
    }

    /// <summary>
    /// 异步批量更新变量。
    /// </summary>
    /// <param name="variables">要更新的变量实体集合。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateBatchAsync(IEnumerable<Variable> variables)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var dbVariables = _mapper.Map<List<DbVariable>>(variables);
        var result = await _dbContext.GetInstance().Updateable(dbVariables).ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Batch update {typeof(DbVariable)}, Count={dbVariables.Count}, 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
}