using DMS.Application.DTOs;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义菜单管理相关的应用服务操作。
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// 异步根据ID获取菜单DTO。
    /// </summary>
    Task<MenuBeanDto> GetMenuByIdAsync(int id);

    /// <summary>
    /// 异步获取所有菜单DTO列表。
    /// </summary>
    Task<List<MenuBeanDto>> GetAllMenusAsync();

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

}