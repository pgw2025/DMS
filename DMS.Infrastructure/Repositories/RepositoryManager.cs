using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Infrastructure.Data;
using SqlSugar;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 仓储管理器实现类，负责管理所有具体的仓储实例，并提供事务管理功能。
/// 实现 <see cref="IRepositoryManager"/> 接口。
/// </summary>
public class RepositoryManager : IRepositoryManager
{
    private readonly SqlSugarClient _db;
    private readonly IMapper _mapper;
    private readonly SqlSugarDbContext _dbContext;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// 在此初始化所有具体的仓储实例。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    public RepositoryManager(IMapper mapper, SqlSugarDbContext dbContext)
    {
        _mapper = mapper;
        _dbContext = dbContext;
        _db = dbContext.GetInstance();

        // 初始化各个仓储实例
        InitializeRepository=new InitializeRepository(dbContext);
        Devices = new DeviceRepository(mapper, dbContext);
        VariableTables = new VariableTableRepository(mapper, dbContext);
        Variables = new VariableRepository(mapper, dbContext);
        MqttServers = new MqttServerRepository(mapper, dbContext);
        VariableMqttAliases = new VariableMqttAliasRepository(mapper, dbContext);
        Menus = new MenuRepository(mapper, dbContext);
        VariableHistories = new VariableHistoryRepository(mapper, dbContext);
        Users = new UserRepository(mapper, dbContext);
    }

    /// <summary>
    /// 释放数据库连接资源。
    /// </summary>
    public void Dispose()
    {
        _db.Close();
    }

    /// <summary>
    /// 获取设备仓储实例。
    /// </summary>
    public IDeviceRepository Devices { get; set; }
    /// <summary>
    /// 获取变量表仓储实例。
    /// </summary>
    public IVariableTableRepository VariableTables { get; set; }
    /// <summary>
    /// 获取变量仓储实例。
    /// </summary>
    public IVariableRepository Variables { get; set; }
    /// <summary>
    /// 获取MQTT服务器仓储实例。
    /// </summary>
    public IMqttServerRepository MqttServers { get; set; }
    /// <summary>
    /// 获取变量MQTT别名仓储实例。
    /// </summary>
    public IVariableMqttAliasRepository VariableMqttAliases { get; set; }
    /// <summary>
    /// 获取菜单仓储实例。
    /// </summary>
    public IMenuRepository Menus { get; set; }
    /// <summary>
    /// 获取变量历史仓储实例。
    /// </summary>
    public IVariableHistoryRepository VariableHistories { get; set; }
    /// <summary>
    /// 获取用户仓储实例。
    /// </summary>
    public IUserRepository Users { get; set; }
    /// <summary>
    /// 获取初始化仓储实例。
    /// </summary>
    public IInitializeRepository  InitializeRepository { get; set; }

    /// <summary>
    /// 异步开始数据库事务。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public async Task BeginTranAsync() => await _db.BeginTranAsync();

    /// <summary>
    /// 异步提交数据库事务。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public async Task CommitAsync() => await _db.CommitTranAsync();

    /// <summary>
    /// 异步回滚数据库事务。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public async Task RollbackAsync() => await _db.RollbackTranAsync();
}