using AutoMapper;
using DMS.Core.Helper;
using DMS.Infrastructure.Interfaces;
using DMS.Infrastructure.Data;
using SqlSugar;
using System.Diagnostics;
using System.Linq.Expressions;

namespace DMS.Infrastructure.Repositories;

public abstract class BaseRepository<TEntity, TModel>
    where TEntity : class, new()
    where TModel : class, new()
{
    protected readonly IMapper _mapper;
    private readonly SqlSugarDbContext dbContext;

    protected SqlSugarClient Db => dbContext.GetInstance();

    protected BaseRepository(IMapper mapper, ITransaction transaction)
    {
        _mapper = mapper;
        this.dbContext = dbContext;
    }

    public virtual async Task<int> AddAsync(TModel model)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var entity = _mapper.Map<TEntity>(model);
        var result = await Db.Insertable(entity).ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Add {typeof(TModel).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public virtual async Task<int> UpdateAsync(TModel model)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var entity = _mapper.Map<TEntity>(model);
        var result = await Db.Updateable(entity).ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Update {typeof(TModel).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public virtual async Task<int> DeleteAsync(TModel model)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var entity = _mapper.Map<TEntity>(model);
        var result = await Db.Deleteable(entity).ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(TModel).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public virtual async Task<List<TModel>> GetAllAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var entities = await Db.Queryable<TEntity>().ToListAsync();
        stopwatch.Stop();
        NlogHelper.Info($"GetAll {typeof(TModel).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return _mapper.Map<List<TModel>>(entities);
    }

    public virtual async Task<TModel> GetByIdAsync(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var entity = await Db.Queryable<TEntity>().In(id).FirstAsync();
        stopwatch.Stop();
        NlogHelper.Info($"GetById {typeof(TModel).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return _mapper.Map<TModel>(entity);
    }

    public virtual async Task<TModel> GetByConditionAsync(Expression<Func<TEntity, bool>> expression)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var entity = await Db.Queryable<TEntity>().FirstAsync(expression);
        stopwatch.Stop();
        NlogHelper.Info($"GetByCondition {typeof(TModel).Name}耗时：{stopwatch.ElapsedMilliseconds}ms");
        return _mapper.Map<TModel>(entity);
    }
}
