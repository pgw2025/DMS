using DMS.Core.Interfaces.Repositories;
using DMS.Core.Interfaces.Repositories.Triggers; // 引入新的接口命名空间

namespace DMS.Core.Interfaces;

/// <summary>
/// 定义了一个仓储管理器，它使用工作单元模式来组合多个仓储操作，以确保事务的原子性。
/// 实现了IDisposable，以确保数据库连接等资源能被正确释放。
/// </summary>
public interface IRepositoryManager : IDisposable
{
    /// <summary>
    /// 获取设备仓储的实例。
    /// 所有通过此管理器获取的仓储都共享同一个数据库上下文和事务。
    /// </summary>
    IDeviceRepository Devices { get; set; }

    /// <summary>
    /// 获取变量表仓储的实例。
    /// </summary>
    IVariableTableRepository VariableTables { get; set; }

    /// <summary>
    /// 获取变量仓储的实例。
    /// </summary>
    IVariableRepository Variables { get; set; }

    /// <summary>
    /// 获取MQTT服务器仓储的实例。
    /// </summary>
    IMqttServerRepository MqttServers { get; set; }

    /// <summary>
    /// 获取变量MQTT别名仓储的实例。
    /// </summary>
    IVariableMqttAliasRepository VariableMqttAliases { get; set; }

    /// <summary>
    /// 获取菜单仓储的实例。
    /// </summary>
    IMenuRepository Menus { get; set; }

    /// <summary>
    /// 获取变量历史仓储的实例。
    /// </summary>
    IVariableHistoryRepository VariableHistories { get; set; }

    /// <summary>
    /// 获取用户仓储的实例。
    /// </summary>
    IUserRepository Users { get; set; }

    /// <summary>
    /// 获取Nlog日志仓储的实例。
    /// </summary>
    INlogRepository Nlogs { get; set; }

    /// <summary>
    /// 获取触发器仓储的实例。
    /// </summary>
    ITriggerRepository Triggers { get; set; }

    /// <summary>
    /// 初始化数据库
    /// </summary>
    IInitializeRepository InitializeRepository { get; set; }

    /// <summary>
    /// 开始一个新的数据库事务。
    /// </summary>
    Task BeginTranAsync();

    /// <summary>
    /// 异步提交当前事务中的所有变更。
    /// </summary>
    /// <returns>一个表示异步操作的任务。</returns>
    Task CommitAsync();

    /// <summary>
    /// 异步回滚当前事务中的所有变更。
    /// </summary>
    /// <returns>一个表示异步操作的任务。</returns>
    Task RollbackAsync();
}