using System.Diagnostics;
using DMS.Core.Interfaces.Repositories;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;


/// <summary>
///     变量与MQTT服务器别名关联的数据仓库。
/// </summary>
using AutoMapper;
using DMS.Core.Helper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

/// <summary>
///     变量与MQTT服务器别名关联的数据仓库。
/// </summary>
public class VariableMqttAliasRepository : BaseRepository<DbVariableMqttAlias>, IVariableMqttAliasRepository
{
    private readonly IMapper _mapper;

    public VariableMqttAliasRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
    {
        _mapper = mapper;
    }

    public async Task<VariableMqttAlias> GetByIdAsync(int id)
    {
        var dbVariableMqttAlias = await base.GetByIdAsync(id);
        return _mapper.Map<VariableMqttAlias>(dbVariableMqttAlias);
    }

    public async Task<List<VariableMqttAlias>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<VariableMqttAlias>>(dbList);
    }

    public async Task<VariableMqttAlias> AddAsync(VariableMqttAlias entity)
    {
        var dbVariableMqttAlias = await base.AddAsync(_mapper.Map<DbVariableMqttAlias>(entity));
        return _mapper.Map(dbVariableMqttAlias, entity);
    }

    public async Task<int> UpdateAsync(VariableMqttAlias entity) => await base.UpdateAsync(_mapper.Map<DbVariableMqttAlias>(entity));

    public async Task<int> DeleteAsync(VariableMqttAlias entity) => await base.DeleteAsync(_mapper.Map<DbVariableMqttAlias>(entity));
    
    public async Task<int> DeleteAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new VariableMqttAlias() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbMenu)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
}