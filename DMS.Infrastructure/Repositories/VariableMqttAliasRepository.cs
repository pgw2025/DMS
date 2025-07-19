using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Interfaces;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Data;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 变量与MQTT服务器别名关联的数据仓库。
/// </summary>
public class VariableMqttAliasRepository : BaseRepository<DbVariableMqtt>
{
    public VariableMqttAliasRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
    }
}
