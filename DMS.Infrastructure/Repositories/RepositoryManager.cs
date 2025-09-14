using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Interfaces.Repositories.Triggers; // 引入新的接口
using DMS.Infrastructure.Data;
using SqlSugar;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 仓储管理器实现类，负责管理所有具体的仓储实例，并提供事务管理功能。
/// 实现 <see cref="IRepositoryManager"/> 接口。
/// </summary>
public class RepositoryManager : IRepositoryManager
{
    private readonly SqlSugarDbContext _dbContext;
    private readonly SqlSugarClient _db;

    /// <summary>
    /// 构造函数，注入所有仓储实例。
    /// </summary>
    /// <param name="initializeRepository">初始化仓储实例。</param>
    /// <param name="devices">设备仓储实例。</param>
    /// <param name="variableTables">变量表仓储实例。</param>
    /// <param name="variables">变量仓储实例。</param>
    /// <param name="mqttServers">MQTT服务器仓储实例。</param>
    /// <param name="variableMqttAliases">变量MQTT别名仓储实例。</param>
    /// <param name="menus">菜单仓储实例。</param>
    /// <param name="variableHistories">变量历史仓储实例。</param>
    /// <param name="users">用户仓储实例。</param>
    /// <param name="nlogs">Nlog日志仓储实例。</param>
    /// <param name="triggers">触发器仓储实例。</param>
    public RepositoryManager( SqlSugarDbContext dbContext,
        IInitializeRepository initializeRepository,
        IDeviceRepository devices,
        IVariableTableRepository variableTables,
        IVariableRepository variables,
        IMqttServerRepository mqttServers,
        IVariableMqttAliasRepository variableMqttAliases,
        IMenuRepository menus,
        IVariableHistoryRepository variableHistories,
        IUserRepository users,
        INlogRepository nlogs,
        ITriggerRepository triggers) // 新增参数
    {
        _dbContext = dbContext;
        InitializeRepository = initializeRepository;
        Devices = devices;
        VariableTables = variableTables;
        Variables = variables;
        MqttServers = mqttServers;
        VariableMqttAliases = variableMqttAliases;
        Menus = menus;
        VariableHistories = variableHistories;
        Users = users;
        Nlogs = nlogs;
        Triggers = triggers; // 赋值
        
       _db = dbContext.GetInstance();
    }

    /// <summary>
    /// 释放数据库连接资源。
    /// </summary>
    public void Dispose()
    {
        _db?.Close();
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
    /// 获取Nlog日志仓储实例。
    /// </summary>
    public INlogRepository Nlogs { get; set; }
    /// <summary>
    /// 获取触发器仓储实例。
    /// </summary>
    public ITriggerRepository Triggers { get; set; }
    /// <summary>
    /// 获取初始化仓储实例。
    /// </summary>
    public IInitializeRepository  InitializeRepository { get; set; }

    /// <summary>
    /// 异步开始数据库事务。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public async Task BeginTranAsync() 
    {
        if (_db != null)
            await _db.BeginTranAsync();
    }

    /// <summary>
    /// 异步提交数据库事务。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public async Task CommitAsync() 
    {
        if (_db != null)
            await _db.CommitTranAsync();
    }

    /// <summary>
    /// 异步回滚数据库事务。
    /// </summary>
    /// <returns>表示异步操作的任务。</returns>
    public async Task RollbackAsync() 
    {
        if (_db != null)
            await _db.RollbackTranAsync();
    }
}