using System.Diagnostics;
using AutoMapper;
using DMS.Core.Helper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

/// <summary>
///     Mqtt仓储类，用于操作DbMqtt实体
/// </summary>
public class MqttServerRepository : BaseRepository<DbMqttServer>, IMqttServerRepository
{
    private readonly IMapper _mapper;

    public MqttServerRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
    {
        _mapper = mapper;
    }

    public async Task<MqttServer> GetByIdAsync(int id)
    {
        var dbMqttServer = await base.GetByIdAsync(id);
        return _mapper.Map<MqttServer>(dbMqttServer);
    }

    public async Task<List<MqttServer>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<MqttServer>>(dbList);
    }

    public async Task<MqttServer> AddAsync(MqttServer entity)
    {
        var dbMqttServer = await base.AddAsync(_mapper.Map<DbMqttServer>(entity));
        return _mapper.Map(dbMqttServer, entity);
    }

    public async Task<int> UpdateAsync(MqttServer entity) => await base.UpdateAsync(_mapper.Map<DbMqttServer>(entity));

    public async Task<int> DeleteAsync(MqttServer entity) => await base.DeleteAsync(_mapper.Map<DbMqttServer>(entity));
    
    public async Task<int> DeleteAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new MqttServer() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(MqttServer)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
}