namespace DMS.Application.Services;

public interface IInitializeService
{
    /// <summary>
    /// 初始化数据库表。
    /// </summary>
    void InitializeTables();

    /// <summary>
    /// 初始化数据库表索引。
    /// </summary>
    void InitializeTableIndex();

    /// <summary>
    /// 初始化默认菜单。
    /// </summary>
    void InitializeMenus();
}