using AutoMapper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using Microsoft.Extensions.Logging;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// Nlog日志仓储实现类，负责Nlog日志数据的持久化操作。
/// 继承自 <see cref="BaseRepository{DbNlog}"/> 并实现 <see cref="INlogRepository"/> 接口。
/// </summary>
public class NlogRepository : BaseRepository<DbNlog>, INlogRepository
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    /// <param name="logger">日志记录器实例。</param>
    public NlogRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<NlogRepository> logger)
        : base(dbContext, logger)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 异步删除所有Nlog日志。
    /// </summary>
    public async Task DeleteAllAsync()
    {
        await _dbContext.GetInstance().Deleteable<DbNlog>().ExecuteCommandAsync();
    }

    // Nlog 通常是只读或追加的日志，因此像 AddAsync, UpdateAsync, DeleteAsync 这样的修改方法
    // 可能不需要在仓储接口中暴露，或者可以省略具体实现或抛出 NotSupportedException。
    // 但为了保持与基类一致性并满足接口要求，这里显式实现它们。

    /// <summary>
    /// 异步获取所有Nlog日志条目。
    /// </summary>
    /// <returns>包含所有Nlog日志实体的列表。</returns>
    public new async Task<List<Core.Models.Nlog>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<Core.Models.Nlog>>(dbList);
    }

    /// <summary>
    /// 异步根据ID获取单个Nlog日志条目。
    /// </summary>
    /// <param name="id">日志条目的唯一标识符。</param>
    /// <returns>对应的Nlog日志实体，如果不存在则为null。</returns>
    public new async Task<Core.Models.Nlog> GetByIdAsync(int id)
    {
        var dbNlog = await base.GetByIdAsync(id);
        return _mapper.Map<Core.Models.Nlog>(dbNlog);
    }

    /// <summary>
    /// 异步添加一个新Nlog日志条目。
    /// 注意：直接写入数据库日志表通常不推荐，NLog本身负责写入。
    /// 此方法主要用于满足接口契约，实际使用应谨慎。
    /// </summary>
    /// <param name="entity">要添加的Nlog日志实体。</param>
    /// <returns>添加后的Nlog日志实体（包含ID等信息）。</returns>
    public new async Task<Core.Models.Nlog> AddAsync(Core.Models.Nlog entity)
    {
        var dbEntity = _mapper.Map<DbNlog>(entity);
        var addedDbEntity = await base.AddAsync(dbEntity);
        return _mapper.Map(addedDbEntity, entity);
    }

    /// <summary>
    /// 异步更新一个Nlog日志条目。
    /// 注意：直接更新数据库日志表通常不推荐，日志应视为不可变。
    /// 此方法主要用于满足接口契约，实际使用应谨慎。
    /// </summary>
    /// <param name="entity">要更新的Nlog日志实体。</param>
    /// <returns>受影响的行数。</returns>
    public new async Task<int> UpdateAsync(Core.Models.Nlog entity)
    {
        var dbEntity = _mapper.Map<DbNlog>(entity);
        return await base.UpdateAsync(dbEntity);
    }

    /// <summary>
    /// 异步删除一个Nlog日志条目。
    /// 注意：直接删除数据库日志表记录通常不推荐，除非是清理过期日志。
    /// 此方法主要用于满足接口契约，实际使用应谨慎。
    /// </summary>
    /// <param name="entity">要删除的Nlog日志实体。</param>
    /// <returns>受影响的行数。</returns>
    public new async Task<int> DeleteAsync(Core.Models.Nlog entity)
    {
        var dbEntity = _mapper.Map<DbNlog>(entity);
        return await base.DeleteAsync(dbEntity);
    }

    /// <summary>
    /// 异步根据ID删除一个Nlog日志条目。
    /// 注意：直接删除数据库日志表记录通常不推荐，除非是清理过期日志。
    /// 此方法主要用于满足接口契约，实际使用应谨慎。
    /// </summary>
    /// <param name="id">要删除的Nlog日志条目的ID。</param>
    /// <returns>受影响的行数。</returns>
    public new async Task<int> DeleteByIdAsync(int id)
    {
        return await base.DeleteByIdsAsync(new List<int>(){id});
    }

    /// <summary>
    /// 异步获取指定数量的Nlog日志条目。
    /// </summary>
    /// <param name="number">要获取的条目数量。</param>
    /// <returns>包含指定数量Nlog日志实体的列表。</returns>
    public new async Task<List<Core.Models.Nlog>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<Core.Models.Nlog>>(dbList);
    }

    /// <summary>
    /// 异步批量添加Nlog日志条目。
    /// 注意：直接批量写入数据库日志表通常不推荐，NLog本身负责写入。
    /// 此方法主要用于满足接口契约，实际使用应谨慎。
    /// </summary>
    /// <param name="entities">要添加的Nlog日志实体列表。</param>
    /// <returns>添加的Nlog日志实体列表。</returns>
    public new async Task<List<Core.Models.Nlog>> AddBatchAsync(List<Core.Models.Nlog> entities)
    {
        var dbEntities = _mapper.Map<List<DbNlog>>(entities);
        var addedEntities = await base.AddBatchAsync(dbEntities);
        return _mapper.Map<List<Core.Models.Nlog>>(addedEntities);
    }
}