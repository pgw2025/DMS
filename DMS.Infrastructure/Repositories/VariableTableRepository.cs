using System.Diagnostics;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;


using AutoMapper;
using DMS.Core.Helper;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 变量表仓储实现类，负责变量表数据的持久化操作。
/// 继承自 <see cref="BaseRepository{DbVariableTable}"/> 并实现 <see cref="IVariableTableRepository"/> 接口。
/// </summary>
public class VariableTableRepository : BaseRepository<DbVariableTable>, IVariableTableRepository
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    public VariableTableRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取单个变量表。
    /// </summary>
    /// <param name="id">变量表的唯一标识符。</param>
    /// <returns>对应的变量表实体，如果不存在则为null。</returns>
    public async Task<VariableTable> GetByIdAsync(int id)
    {
        var dbVariableTable = await base.GetByIdAsync(id);
        return _mapper.Map<VariableTable>(dbVariableTable);
    }

    /// <summary>
    /// 异步获取所有变量表。
    /// </summary>
    /// <returns>包含所有变量表实体的列表。</returns>
    public async Task<List<VariableTable>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<VariableTable>>(dbList);
    }

    /// <summary>
    /// 异步添加新变量表。
    /// </summary>
    /// <param name="entity">要添加的变量表实体。</param>
    /// <returns>添加成功后的变量表实体（包含数据库生成的ID等信息）。</returns>
    public async Task<VariableTable> AddAsync(VariableTable entity)
    {
        var dbVariableTable = await base.AddAsync(_mapper.Map<DbVariableTable>(entity));
        return _mapper.Map(dbVariableTable, entity);
    }

    /// <summary>
    /// 异步更新现有变量表。
    /// </summary>
    /// <param name="entity">要更新的变量表实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAsync(VariableTable entity) => await base.UpdateAsync(_mapper.Map<DbVariableTable>(entity));

    /// <summary>
    /// 异步删除变量表。
    /// </summary>
    /// <param name="entity">要删除的变量表实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(VariableTable entity) => await base.DeleteAsync(_mapper.Map<DbVariableTable>(entity));
    
    /// <summary>
    /// 异步根据ID删除变量表。
    /// </summary>
    /// <param name="id">要删除变量表的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new DbVariableTable() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbVariableTable)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 异步获取指定数量的变量表。
    /// </summary>
    /// <param name="number">要获取的变量表数量。</param>
    /// <returns>包含指定数量变量表实体的列表。</returns>
    public new async Task<List<VariableTable>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<VariableTable>>(dbList);

    }

    public Task<bool> AddBatchAsync(List<VariableTable> entities)
    {
        var dbEntities = _mapper.Map<List<DbVariableTable>>(entities);
        return base.AddBatchAsync(dbEntities);
    }

    /// <summary>
    /// 异步根据设备ID删除所有关联的变量表。
    /// </summary>
    /// <param name="deviceId">设备的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByDeviceIdAsync(int deviceId)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable<DbVariableTable>()
                             .Where(it => it.DeviceId == deviceId)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete VariableTable by DeviceId={deviceId}, 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
}