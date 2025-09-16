using DMS.Application.DTOs;

namespace DMS.Application.Interfaces.Management;

public interface IMenuManagementService
{
    /// <summary>
    /// 异步获取所有菜单DTO列表。
    /// </summary>
    Task<List<MenuBeanDto>> GetAllMenusAsync();

    /// <summary>
    /// 异步根据ID获取菜单DTO。
    /// </summary>
    Task<MenuBeanDto> GetMenuByIdAsync(int id);

    /// <summary>
    /// 异步创建一个新菜单。
    /// </summary>
    Task<int> CreateMenuAsync(MenuBeanDto menuDto);

    /// <summary>
    /// 异步更新一个已存在的菜单。
    /// </summary>
    Task UpdateMenuAsync(MenuBeanDto menuDto);

    /// <summary>
    /// 异步删除一个菜单。
    /// </summary>
    Task DeleteMenuAsync(int id);

    /// <summary>
    /// 在内存中添加菜单
    /// </summary>
    void AddMenuToMemory(MenuBeanDto menuDto);

    /// <summary>
    /// 在内存中更新菜单
    /// </summary>
    void UpdateMenuInMemory(MenuBeanDto menuDto);

    /// <summary>
    /// 在内存中删除菜单
    /// </summary>
    void RemoveMenuFromMemory(int menuId);

    /// <summary>
    /// 获取根菜单列表
    /// </summary>
    List<MenuBeanDto> GetRootMenus();

    /// <summary>
    /// 根据父级ID获取子菜单列表
    /// </summary>
    /// <param name="parentId">父级菜单ID</param>
    /// <returns>子菜单列表</returns>
    List<MenuBeanDto> GetChildMenus(int parentId);

    /// <summary>
    /// 构建菜单树结构
    /// </summary>
    void BuildMenuTree();
}