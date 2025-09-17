using System.Diagnostics;
using System.Linq.Expressions;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 通用仓储基类，封装了对实体对象的常用 CRUD 操作。
/// </summary>
/// <typeparam name="TEntity">实体类型，必须是引用类型且具有无参构造函数。</typeparam>
public abstract class BaseRepository<TEntity>
    where TEntity : class, new()
{
    protected readonly SqlSugarDbContext _dbContext;
    protected readonly ILogger<BaseRepository<TEntity>> _logger;

    /// <summary>
    /// 初始化 BaseRepository 的新实例。
    /// </summary>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    /// <param name="logger">日志记录器实例。</param>
    protected BaseRepository(SqlSugarDbContext dbContext, ILogger<BaseRepository<TEntity>> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 获取当前事务的 SqlSugarClient 实例，用于数据库操作。
    /// </summary>
    protected SqlSugarClient Db
    {
        get { return _dbContext.GetInstance(); }
    }

    /// <summary>
    /// 异步添加一个新实体。
    /// </summary>
    /// <param name="entity">要添加的实体对象。</param>
    /// <returns>返回已添加的实体对象（可能包含数据库生成的主键等信息）。</returns>
    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Insertable(entity)
                             .ExecuteReturnEntityAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Add {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 异步更新一个现有实体。
    /// </summary>
    /// <param name="entity">要更新的实体对象。</param>
    /// <returns>返回受影响的行数。</returns>
    public virtual async Task<int> UpdateAsync(TEntity entity)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Updateable(entity)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Update {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 异步删除一个实体。
    /// </summary>
    /// <param name="entity">要删除的实体对象。</param>
    /// <returns>返回受影响的行数。</returns>
    public virtual async Task<int> DeleteAsync(TEntity entity)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Deleteable(entity)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Delete {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    

    /// <summary>
    /// 异步获取所有实体。
    /// </summary>
    /// <returns>返回包含所有实体的列表。</returns>
    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var entities = await _dbContext.GetInstance().Queryable<TEntity>()
                               .ToListAsync();
        stopwatch.Stop();
        _logger.LogInformation($"GetAll {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return entities;
    }

   

    /// <summary>
    /// 异步根据主键 ID (int类型) 获取单个实体。
    /// </summary>
    /// <param name="id">实体的主键 ID (int类型)。</param>
    /// <returns>返回找到的实体，如果未找到则返回 null。</returns>
    public virtual async Task<TEntity> GetByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var entity = await _dbContext.GetInstance().Queryable<TEntity>()
                             .In(id)
                             .FirstAsync();
        stopwatch.Stop();
        _logger.LogInformation($"GetById {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return entity;
    }

    /// <summary>
    /// 异步根据主键 ID (Guid类型) 获取单个实体。
    /// </summary>
    /// <param name="id">实体的主键 ID (Guid类型)。</param>
    /// <returns>返回找到的实体，如果未找到则返回 null。</returns>
    public virtual async Task<TEntity> GetByIdAsync(Guid id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var entity = await _dbContext.GetInstance().Queryable<TEntity>()
                             .In(id)
                             .FirstAsync();
        stopwatch.Stop();
        _logger.LogInformation($"GetById {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return entity;
    }

    /// <summary>
    /// 异步根据主键 ID 列表批量删除实体。
    /// </summary>
    /// <param name="ids">要删除的实体主键 ID 列表。</param>
    /// <returns>返回受影响的行数。</returns>
    public virtual async Task<int> DeleteByIdsAsync(List<int> ids)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Deleteable<TEntity>()
                             .In(ids)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"DeleteByIds {typeof(TEntity).Name}, Count: {ids.Count}, 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 异步根据指定条件获取单个实体。
    /// </summary>
    /// <param name="expression">查询条件的 Lambda 表达式。</param>
    /// <returns>返回满足条件的第一个实体，如果未找到则返回 null。</returns>
    public virtual async Task<TEntity> GetByConditionAsync(Expression<Func<TEntity, bool>> expression)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var entity = await _dbContext.GetInstance().Queryable<TEntity>()
                             .FirstAsync(expression);
        stopwatch.Stop();
        _logger.LogInformation($"GetByCondition {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return entity;
    }

    /// <summary>
    /// 异步判断是否存在满足条件的实体。
    /// </summary>
    /// <param name="expression">查询条件的 Lambda 表达式。</param>
    /// <returns>如果存在则返回 true，否则返回 false。</returns>
    public virtual async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Queryable<TEntity>()
                             .AnyAsync(expression);
        stopwatch.Stop();
        _logger.LogInformation($"Exists {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 异步开始数据库事务。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public async Task BeginTranAsync()
    {
        await _dbContext.GetInstance().BeginTranAsync();
    }

    /// <summary>
    /// 异步提交数据库事务。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public async Task CommitTranAsync()
    {
        await _dbContext.GetInstance().CommitTranAsync();
    }


    /// <summary>
    /// 异步回滚数据库事务。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public async Task RollbackTranAsync()
    {
        await _dbContext.GetInstance().RollbackTranAsync();
    }

    /// <summary>
    /// 异步获取指定数量的实体。
    /// </summary>
    /// <param name="number">要获取的实体数量。</param>
    /// <returns>包含指定数量实体对象的列表。</returns>
    public virtual async Task<List<TEntity>> TakeAsync(int number)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var entity = await _dbContext.GetInstance().Queryable<TEntity>().Take(number).ToListAsync();
                            
        stopwatch.Stop();
        _logger.LogInformation($"TakeAsync {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return entity;
    }

    /// <summary>
    /// 异步根据主键 ID 删除单个实体。
    /// </summary>
    /// <param name="id">要删除的实体的主键 ID。</param>
    /// <returns>返回受影响的行数。</returns>
    public virtual async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Deleteable<TEntity>()
                             .In(id)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"DeleteById {typeof(TEntity).Name}, ID: {id}, 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public async Task<List<TEntity>> AddBatchAsync(List<TEntity> entities)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var retrunEntities = new  List<TEntity>();
        foreach (var entity in entities)
        {
            var result = await _dbContext.GetInstance().Insertable(entity).ExecuteReturnEntityAsync();
            retrunEntities.Add(result);
        }

        stopwatch.Stop();
        _logger.LogInformation($"AddBatchAsync {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        
       
        
        return retrunEntities;
    }
}