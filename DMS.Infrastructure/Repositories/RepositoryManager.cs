using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Infrastructure.Data;
using SqlSugar;

namespace DMS.Infrastructure.Repositories;

public class RepositoryManager : IRepositoryManager
{
    private readonly SqlSugarClient _db;
    private readonly IMapper _mapper;
    private readonly SqlSugarDbContext _dbContext;

    public RepositoryManager(IMapper mapper, SqlSugarDbContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _db = dbContext.GetInstance();

        Devices = new DeviceRepository(mapper, dbContext);
        VariableTables = new VariableTableRepository(mapper, dbContext);
        Variables = new VariableRepository(mapper, dbContext);
        MqttServers = new MqttServerRepository(mapper, dbContext);
        VariableMqttAliases = new VariableMqttAliasRepository(mapper, dbContext);
        Menus = new MenuRepository(mapper, dbContext);
        VariableHistories = new VariableHistoryRepository(mapper, dbContext);
        Users = new UserRepository(mapper, dbContext);
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