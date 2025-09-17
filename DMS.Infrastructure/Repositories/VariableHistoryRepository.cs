using System.Diagnostics;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;


using AutoMapper;
using Microsoft.Extensions.Logging;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 变量历史仓储实现类，负责变量历史数据的持久化操作。
/// 继承自 <see cref="BaseRepository{DbVariableHistory}"/> 并实现 <see cref="IVariableHistoryRepository"/> 接口。
/// </summary>
public class VariableHistoryRepository : BaseRepository<DbVariableHistory>, IVariableHistoryRepository
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    /// <param name="logger">日志记录器实例。</param>
    public VariableHistoryRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<VariableHistoryRepository> logger)
        : base(dbContext, logger)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取单个变量历史记录。
    /// </summary>
    /// <param name="id">变量历史记录的唯一标识符。</param>
    /// <returns>对应的变量历史实体，如果不存在则为null。</returns>
    public async Task<VariableHistory> GetByIdAsync(int id)
    {
        var dbVariableHistory = await base.GetByIdAsync(id);
        return _mapper.Map<VariableHistory>(dbVariableHistory);
    }

    /// <summary>
    /// 异步获取所有变量历史记录。
    /// </summary>
    /// <returns>包含所有变量历史实体的列表。</returns>
    public async Task<List<VariableHistory>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<VariableHistory>>(dbList);
    }

    /// <summary>
    /// 异步添加新变量历史记录。
    /// </summary>
    /// <param name="entity">要添加的变量历史实体。</param>
    /// <returns>添加成功后的变量历史实体（包含数据库生成的ID等信息）。</returns>
    public async Task<VariableHistory> AddAsync(VariableHistory entity)
    {
        var dbVariableHistory = await base.AddAsync(_mapper.Map<DbVariableHistory>(entity));
        return _mapper.Map(dbVariableHistory, entity);
    }

    /// <summary>
    /// 异步更新现有变量历史记录。
    /// </summary>
    /// <param name="entity">要更新的变量历史实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAsync(VariableHistory entity) => await base.UpdateAsync(_mapper.Map<DbVariableHistory>(entity));

    /// <summary>
    /// 异步删除变量历史记录。
    /// </summary>
    /// <param name="entity">要删除的变量历史实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(VariableHistory entity) => await base.DeleteAsync(_mapper.Map<DbVariableHistory>(entity));
    
    /// <summary>
    /// 异步根据ID删除变量历史记录。
    /// </summary>
    /// <param name="id">要删除变量历史记录的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Deleteable(new DbVariableHistory() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Delete {typeof(DbVariableHistory)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 异步获取指定数量的变量历史记录。
    /// </summary>
    /// <param name="number">要获取的变量历史记录数量。</param>
    /// <returns>包含指定数量变量历史实体的列表。</returns>
    public new async Task<List<VariableHistory>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<VariableHistory>>(dbList);

    }

    public async Task<List<VariableHistory>> AddBatchAsync(List<VariableHistory> entities)
    {
        var dbEntities = _mapper.Map<List<DbVariableHistory>>(entities);
        var addedEntities = await base.AddBatchAsync(dbEntities);
        return _mapper.Map<List<VariableHistory>>(addedEntities);
    }
    
    /// <summary>
    /// 根据变量ID获取历史记录
    /// </summary>
    /// <param name="variableId">变量ID</param>
    /// <returns>变量历史记录列表</returns>
    public async Task<List<VariableHistory>> GetByVariableIdAsync(int variableId)
    {
        var dbList = await _dbContext.GetInstance().Queryable<DbVariableHistory>()
                             .Where(h => h.VariableId == variableId)
                             .OrderBy(h => h.Timestamp, SqlSugar.OrderByType.Desc)
                             .ToListAsync();
        return _mapper.Map<List<VariableHistory>>(dbList);
    }
    
    /// <summary>
    /// 根据变量ID获取历史记录，支持条数限制和时间范围筛选
    /// </summary>
    /// <param name="variableId">变量ID</param>
    /// <param name="limit">返回记录的最大数量，null表示无限制</param>
    /// <param name="startTime">开始时间，null表示无限制</param>
    /// <param name="endTime">结束时间，null表示无限制</param>
    /// <returns>变量历史记录列表</returns>
    public async Task<List<VariableHistory>> GetByVariableIdAsync(int variableId, int? limit = null, DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = _dbContext.GetInstance().Queryable<DbVariableHistory>()
                      .Where(h => h.VariableId == variableId);
        
        // 添加时间范围筛选
        if (startTime.HasValue)
            query = query.Where(h => h.Timestamp >= startTime.Value);
        
        if (endTime.HasValue)
            query = query.Where(h => h.Timestamp <= endTime.Value);
        
        // 按时间倒序排列
        query = query.OrderBy(h => h.Timestamp, SqlSugar.OrderByType.Desc);
        
        // 添加条数限制
        if (limit.HasValue)
            query = query.Take(limit.Value);
        
        var dbList = await query.ToListAsync();
        return _mapper.Map<List<VariableHistory>>(dbList);
    }
    
    /// <summary>
    /// 获取所有历史记录，支持条数限制和时间范围筛选
    /// </summary>
    /// <param name="limit">返回记录的最大数量，null表示无限制</param>
    /// <param name="startTime">开始时间，null表示无限制</param>
    /// <param name="endTime">结束时间，null表示无限制</param>
    /// <returns>所有历史记录列表</returns>
    public new async Task<List<VariableHistory>> GetAllAsync(int? limit = null, DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = _dbContext.GetInstance().Queryable<DbVariableHistory>();
        
        // 添加时间范围筛选
        if (startTime.HasValue)
            query = query.Where(h => h.Timestamp >= startTime.Value);
        
        if (endTime.HasValue)
            query = query.Where(h => h.Timestamp <= endTime.Value);
        
        // 按时间倒序排列
        query = query.OrderBy(h => h.Timestamp, SqlSugar.OrderByType.Desc);
        
        // 添加条数限制
        if (limit.HasValue)
            query = query.Take(limit.Value);
        
        var dbList = await query.ToListAsync();
        return _mapper.Map<List<VariableHistory>>(dbList);
    }
}