using System.Diagnostics;
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
    public MqttServerRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<MqttServer> GetByIdAsync(int id) => throw new NotImplementedException();

    public async Task<List<MqttServer>> GetAllAsync() => throw new NotImplementedException();

    public async Task<MqttServer> AddAsync(MqttServer entity) => throw new NotImplementedException();

    public async Task<int> UpdateAsync(MqttServer entity) => throw new NotImplementedException();

    public async Task<int> DeleteAsync(MqttServer entity) => throw new NotImplementedException();
}