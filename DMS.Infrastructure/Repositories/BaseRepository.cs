using AutoMapper;
using DMS.Core.Helper;
using DMS.Infrastructure.Interfaces;
using DMS.Infrastructure.Data;
using SqlSugar;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DMS.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity>
    where TEntity : class, new()
{
    private readonly ITransaction _transaction;

    protected SqlSugarClient Db => _transaction.GetInstance();

    protected BaseRepository(ITransaction transaction)
    {
        this._transaction = transaction;
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Insertable(entity).ExecuteReturnEntityAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Add {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public virtual async Task<int> UpdateAsync(TEntity entity)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Updateable(entity).ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Update {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public virtual async Task<int> DeleteAsync(TEntity entity)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(entity).ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public virtual async Task<List<TEntity>> GetAllAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var entities = await Db.Queryable<TEntity>().ToListAsync();
        stopwatch.Stop();
        NlogHelper.Info($"GetAll {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return entities;
    }

    public virtual async Task<TEntity> GetByIdAsync(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var entity = await Db.Queryable<TEntity>().In(id).FirstAsync();
        stopwatch.Stop();
        NlogHelper.Info($"GetById {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return entity;
    }

    public virtual async Task<TEntity> GetByConditionAsync(Expression<Func<TEntity, bool>> expression)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var entity = await Db.Queryable<TEntity>().FirstAsync(expression);
        stopwatch.Stop();
        NlogHelper.Info($"GetByCondition {typeof(TEntity).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return entity;
    }
}
