using System.Diagnostics;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;


using AutoMapper;
using DMS.Core.Helper;

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
    public VariableHistoryRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
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
        var result = await Db.Deleteable(new DbVariableHistory() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbVariableHistory)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
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
}