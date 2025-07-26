using DMS.Core.Interfaces.Repositories;

namespace DMS.Application.Services;

/// <summary>
/// 初始化服务，负责应用程序启动时的数据库和菜单初始化。
/// </summary>
public class InitializeService : IInitializeService
{
    private readonly IInitializeRepository _repository;

    /// <summary>
    /// 构造函数，注入初始化仓储。
    /// </summary>
    /// <param name="repository">初始化仓储实例。</param>
    public InitializeService(IInitializeRepository repository )
    {
        _repository = repository;
    }

    /// <summary>
    /// 初始化数据库表。
    /// </summary>
    public void InitializeTables()
    {
        _repository.InitializeTables();
    }

    /// <summary>
    /// 初始化数据库表索引。
    /// </summary>
    public void InitializeTableIndex()
    {
        _repository.InitializeTableIndex();
    }

    /// <summary>
    /// 初始化默认菜单。
    /// </summary>
    public void InitializeMenus()
    {
        _repository.InitializeMenus();
    }

}