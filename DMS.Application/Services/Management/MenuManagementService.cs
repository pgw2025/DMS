using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;

namespace DMS.Application.Services.Management;

/// <summary>
/// 菜单管理服务，负责菜单相关的业务逻辑。
/// </summary>
public class MenuManagementService : IMenuManagementService
{
    private readonly IMenuAppService _menuService;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly IEventService _eventService;

    /// <summary>
    /// 当菜单数据发生变化时触发
    /// </summary>
    public event EventHandler<MenuChangedEventArgs> MenuChanged;

    public MenuManagementService(IMenuAppService menuService, IAppDataStorageService appDataStorageService, IEventService eventService)
    {
        _menuService = menuService;
        _appDataStorageService = appDataStorageService;
        _eventService = eventService;
    }

    /// <summary>
    /// 异步获取所有菜单DTO列表。
    /// </summary>
    public async Task<List<MenuBeanDto>> GetAllMenusAsync()
    {
        return await _menuService.GetAllMenusAsync();
    }

    /// <summary>
    /// 异步根据ID获取菜单DTO。
    /// </summary>
    public async Task<MenuBeanDto> GetMenuByIdAsync(int id)
    {
        return await _menuService.GetMenuByIdAsync(id);
    }

    /// <summary>
    /// 异步创建一个新菜单。
    /// </summary>
    public async Task<int> CreateMenuAsync(MenuBeanDto menuDto)
    {
        var result = await _menuService.CreateMenuAsync(menuDto);
        
        // 创建成功后，将菜单添加到内存中
        if (result > 0)
        {
            menuDto.Id = result; // 假设返回的ID是新创建的
            if (_appDataStorageService.Menus.TryAdd(menuDto.Id, menuDto))
            {
                MenuBeanDto parentMenu = null;
                if (menuDto.ParentId > 0 && _appDataStorageService.Menus.TryGetValue(menuDto.ParentId, out var parent))
                {
                    parentMenu = parent;
                    parent.Children.Add(menuDto);
                }

                _eventService.RaiseMenuChanged(this, new MenuChangedEventArgs(DataChangeType.Added, menuDto));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步更新一个已存在的菜单。
    /// </summary>
    public async Task<int> UpdateMenuAsync(MenuBeanDto menuDto)
    {
        var result = await _menuService.UpdateMenuAsync(menuDto);
        
        // 更新成功后，更新内存中的菜单
        if (result > 0 && menuDto != null)
        {
            _appDataStorageService.Menus.AddOrUpdate(menuDto.Id, menuDto, (key, oldValue) => menuDto);
            

            _eventService.RaiseMenuChanged(this, new MenuChangedEventArgs(DataChangeType.Updated, menuDto));
        }

        return result;
    }

    /// <summary>
    /// 异步删除一个菜单。
    /// </summary>
    public async Task<bool> DeleteMenuAsync(int id)
    {
        var menu = await _menuService.GetMenuByIdAsync(id); // 获取菜单信息用于内存删除
        var result = await _menuService.DeleteMenuAsync(id);
        
        // 删除成功后，从内存中移除菜单
        if (result && menu != null)
        {
            if (_appDataStorageService.Menus.TryRemove(id, out var menuDto))
            {
                // 从父菜单中移除子菜单
                if (menuDto.ParentId > 0 && _appDataStorageService.Menus.TryGetValue(menuDto.ParentId, out var parentMenu))
                {
                    parentMenu.Children.Remove(menuDto);
                }
                
                _eventService.RaiseMenuChanged(this, new MenuChangedEventArgs(DataChangeType.Deleted, menuDto));
            }
        }

        return result;
    }

    /// <summary>
    /// 获取根菜单列表
    /// </summary>
    public List<MenuBeanDto> GetRootMenus()
    {
        return _appDataStorageService.Menus.Values.Where(m => m.ParentId == 0)
                    .ToList();
    }

    /// <summary>
    /// 根据父级ID获取子菜单列表
    /// </summary>
    /// <param name="parentId">父级菜单ID</param>
    /// <returns>子菜单列表</returns>
    public List<MenuBeanDto> GetChildMenus(int parentId)
    {
        return _appDataStorageService.Menus.Values.Where(m => m.ParentId == parentId)
                    .ToList();
    }

    /// <summary>
    /// 构建菜单树结构
    /// </summary>
    public void BuildMenuTree()
    {
        // 清空现有菜单树
        _appDataStorageService.MenuTrees.Clear();

        // 获取所有根菜单
        var rootMenus = GetRootMenus();

        // 将根菜单添加到菜单树中
        foreach (var rootMenu in rootMenus)
        {
            _appDataStorageService.MenuTrees.TryAdd(rootMenu.Id, rootMenu);
        }
    }

    /// <summary>
    /// 触发菜单变更事件
    /// </summary>
    protected virtual void OnMenuChanged(MenuChangedEventArgs e)
    {
        _eventService.RaiseMenuChanged(this, e);
    }
}