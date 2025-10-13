using DMS.Core.Models;

namespace DMS.Application.Interfaces.Management;

public interface IMenuManagementService
{
    /// <summary>
    /// 异步获取所有菜单列表。
    /// </summary>
    Task<List<MenuBean>> GetAllMenusAsync();

    /// <summary>
    /// 异步根据ID获取菜单。
    /// </summary>
    Task<MenuBean> GetMenuByIdAsync(int id);

    /// <summary>
    /// 异步创建一个新菜单。
    /// </summary>
    Task<int> CreateMenuAsync(MenuBean menu);

    /// <summary>
    /// 异步更新一个已存在的菜单。
    /// </summary>
    Task<int> UpdateMenuAsync(MenuBean menu);

    /// <summary>
    /// 异步删除一个菜单。
    /// </summary>
    Task<bool> DeleteMenuAsync(int id);

    /// <summary>
    /// 获取根菜单列表
    /// </summary>
    List<MenuBean> GetRootMenus();

    /// <summary>
    /// 根据父级ID获取子菜单列表
    /// </summary>
    /// <param name="parentId">父级菜单ID</param>
    /// <returns>子菜单列表</returns>
    List<MenuBean> GetChildMenus(int parentId);

    /// <summary>
    /// 构建菜单树结构
    /// </summary>
    void BuildMenuTree();

    /// <summary>
    /// 当菜单数据发生变化时触发
    /// </summary>
    event EventHandler<DMS.Application.Events.MenuChangedEventArgs> MenuChanged;
}