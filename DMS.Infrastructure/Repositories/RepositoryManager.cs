using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Infrastructure.Data;
using SqlSugar;

namespace DMS.Infrastructure.Repositories;

public class RepositoryManager : IRepositoryManager
{
    private readonly SqlSugarClient _db;
    private readonly SqlSugarDbContext _dbContext;

    public RepositoryManager(SqlSugarDbContext dbContext)
    {
        _dbContext = dbContext;
        _db = dbContext.GetInstance();

        Devices = new DeviceRepository(dbContext);
        VariableTables = new VariableTableRepository(dbContext);
        Variables = new VariableRepository(dbContext);
        MqttServers = new MqttServerRepository(dbContext);
        VariableMqttAliases = new VariableMqttAliasRepository(dbContext);
        Menus = new MenuRepository(dbContext);
        VariableHistories = new VariableHistoryRepository(dbContext);
        Users = new UserRepository(dbContext);
    }

    public void Dispose()
    {
        _db.Close();
    }

    public IDeviceRepository Devices { get; set; }
    public IVariableTableRepository VariableTables { get; set; }
    public IVariableRepository Variables { get; set; }
    public IMqttServerRepository MqttServers { get; set; }
    public IVariableMqttAliasRepository VariableMqttAliases { get; set; }
    public IMenuRepository Menus { get; set; }
    public IVariableHistoryRepository VariableHistories { get; set; }
    public IUserRepository Users { get; set; }
    public async Task BeginTranAsync() => await _db.BeginTranAsync();

    public async Task CommitAsync() => await _db.CommitTranAsync();

    public async Task RollbackAsync() => await _db.RollbackTranAsync();
}