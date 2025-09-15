using System.Diagnostics;
using AutoMapper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using Microsoft.Extensions.Logging;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 设备仓储实现类，负责设备数据的持久化操作。
/// 继承自 <see cref="BaseRepository{DbDevice}"/> 并实现 <see cref="IDeviceRepository"/> 接口。
/// </summary>
public class DeviceRepository : BaseRepository<DbDevice>, IDeviceRepository
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    /// <param name="logger">日志记录器实例。</param>
    public DeviceRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<DeviceRepository> logger)
        : base(dbContext, logger)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 异步获取所有设备。
    /// </summary>
    /// <returns>包含所有设备实体的列表。</returns>
    public async Task<List<Core.Models.Device>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<Core.Models.Device>>(dbList);
    }

    /// <summary>
    /// 异步根据ID获取单个设备。
    /// </summary>
    /// <param name="id">设备的唯一标识符。</param>
    /// <returns>对应的设备实体，如果不存在则为null。</returns>
    public async Task<Core.Models.Device> GetByIdAsync(int id)
    {
        var dbDevice = await base.GetByIdAsync(id);
        return _mapper.Map<Core.Models.Device>(dbDevice);
    }

    /// <summary>
    /// 异步添加新设备。
    /// </summary>
    /// <param name="model">要添加的设备实体。</param>
    /// <returns>添加成功后的设备实体（包含数据库生成的ID等信息）。</returns>
    public async Task<Core.Models.Device> AddAsync(Core.Models.Device model)
    {
        var dbDevice = await base.AddAsync(_mapper.Map<DbDevice>(model));
        return _mapper.Map(dbDevice, model);
    }

    /// <summary>
    /// 异步更新现有设备。
    /// </summary>
    /// <param name="model">要更新的设备实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAsync(Core.Models.Device model) => await base.UpdateAsync(_mapper.Map<DbDevice>(model));


    /// <summary>
    /// 异步删除设备。
    /// </summary>
    /// <param name="model">要删除的设备实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(Core.Models.Device model) => await base.DeleteAsync(_mapper.Map<DbDevice>(model));

    /// <summary>
    /// 异步根据ID删除设备。
    /// </summary>
    /// <param name="id">要删除设备的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new DbDevice() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Delete {typeof(DbDevice)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 异步获取指定数量的设备。
    /// </summary>
    /// <param name="number">要获取的设备数量。</param>
    /// <returns>包含指定数量设备实体的列表。</returns>
    public new async Task<List<Core.Models.Device>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<Core.Models.Device>>(dbList);

    }

    public async Task<List<Device>> AddBatchAsync(List<Device> entities)
    {
        var dbEntities = _mapper.Map<List<DbDevice>>(entities);
        var addedEntities = await base.AddBatchAsync(dbEntities);
        return _mapper.Map<List<Device>>(addedEntities);
    }
}