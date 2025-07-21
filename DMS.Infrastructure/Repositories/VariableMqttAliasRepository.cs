using DMS.Core.Interfaces.Repositories;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

/// <summary>
///     变量与MQTT服务器别名关联的数据仓库。
/// </summary>
public class VariableMqttAliasRepository : BaseRepository<DbVariableMqttAlias>, IVariableMqttAliasRepository
{
    public VariableMqttAliasRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<VariableMqttAlias> GetByIdAsync(int id) => throw new NotImplementedException();

    public async Task<List<VariableMqttAlias>> GetAllAsync() => throw new NotImplementedException();

    public async Task<VariableMqttAlias> AddAsync(VariableMqttAlias entity) => throw new NotImplementedException();

    public async Task<int> UpdateAsync(VariableMqttAlias entity) => throw new NotImplementedException();

    public async Task<int> DeleteAsync(VariableMqttAlias entity) => throw new NotImplementedException();
}