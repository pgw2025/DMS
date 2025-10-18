using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Application.Services.Management;

/// <summary>
/// 菜单管理服务，负责菜单相关的业务逻辑。
/// </summary>
public class MenuManagementService : IMenuManagementService
{
    private readonly IMenuAppService _menuService;
    private readonly IAppStorageService _appStorageService;
    private readonly IEventService _eventService;

    /// <summary>
    /// 当菜单数据发生变化时触发
    /// </summary>
    public event EventHandler<MenuChangedEventArgs> MenuChanged;

    public MenuManagementService(IMenuAppService menuService, IAppStorageService appStorageService, IEventService eventService)
    {
        _menuService = menuService;
        _appStorageService = appStorageService;
        _eventService = eventService;
    }

    /// <summary>
    /// 异步获取所有菜单列表。
    /// </summary>
    public async Task<List<MenuBean>> GetAllMenusAsync()
    {
        return await _menuService.GetAllMenusAsync();
    }

    /// <summary>
    /// 异步根据ID获取菜单。
    /// </summary>
    public async Task<MenuBean> GetMenuByIdAsync(int id)
    {
        return await _menuService.GetMenuByIdAsync(id);
    }

    /// <summary>
    /// 异步创建一个新菜单。
    /// </summary>
    public async Task<int> CreateMenuAsync(MenuBean menu)
    {
        var result = await _menuService.CreateMenuAsync(menu);
        
        // 创建成功后，将菜单添加到内存中
        if (result > 0)
        {
            menu.Id = result; // 假设返回的ID是新创建的
            if (_appStorageService.Menus.TryAdd(menu.Id, menu))
            {
                MenuBean parentMenu = null;
                if (menu.ParentId > 0 && _appStorageService.Menus.TryGetValue(menu.ParentId.Value, out var parent))
                {
                    parentMenu = parent;
                    parent.Children.Add(menu);
                }

                _eventService.RaiseMenuChanged(this, new MenuChangedEventArgs(DataChangeType.Added, menu));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步更新一个已存在的菜单。
    /// </summary>
    public async Task<int> UpdateMenuAsync(MenuBean menu)
    {
        var result = await _menuService.UpdateMenuAsync(menu);
        
        // 更新成功后，更新内存中的菜单
        if (result > 0 && menu != null)
        {
            _appStorageService.Menus.AddOrUpdate(menu.Id, menu, (key, oldValue) => menu);
            

            _eventService.RaiseMenuChanged(this, new MenuChangedEventArgs(DataChangeType.Updated, menu));
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
            if (_appStorageService.Menus.TryRemove(id, out var menuData))
            {
                // 从父菜单中移除子菜单
                if (menuData.ParentId > 0 && _appStorageService.Menus.TryGetValue(menuData.ParentId.Value, out var parentMenu))
                {
                    parentMenu.Children.Remove(menuData);
                }
                
                _eventService.RaiseMenuChanged(this, new MenuChangedEventArgs(DataChangeType.Deleted, menuData));
            }
        }

        return result;
    }

    /// <summary>
    /// 获取根菜单列表
    /// </summary>
    public List<MenuBean> GetRootMenus()
    {
        return _appStorageService.Menus.Values.Where(m => m.ParentId == 0)
                    .ToList();
    }

    /// <summary>
    /// 根据父级ID获取子菜单列表
    /// </summary>
    /// <param name="parentId">父级菜单ID</param>
    /// <returns>子菜单列表</returns>
    public List<MenuBean> GetChildMenus(int parentId)
    {
        return _appStorageService.Menus.Values.Where(m => m.ParentId == parentId)
                    .ToList();
    }

    /// <summary>
    /// 构建菜单树结构
    /// </summary>
    public void BuildMenuTree()
    {
        // 清空现有菜单树
        _appStorageService.MenuTrees.Clear();

        // 获取所有根菜单
        var rootMenus = GetRootMenus();

        // 将根菜单添加到菜单树中
        foreach (var rootMenu in rootMenus)
        {
            _appStorageService.MenuTrees.TryAdd(rootMenu.Id, rootMenu);
        }
    }

    /// <summary>
    /// 触发菜单变更事件
    /// </summary>
    protected virtual void OnMenuChanged(MenuChangedEventArgs e)
    {
        _eventService.RaiseMenuChanged(this, e);
    }

    /// <summary>
    /// 异步加载所有菜单数据到内存中。
    /// </summary>
    public async Task LoadAllMenusAsync()
    {
        _appStorageService.Menus.Clear();
        _appStorageService.MenuTrees.Clear();
        var menus = await _menuService.GetAllMenusAsync();
        // 将菜单添加到安全字典
        foreach (var menuBean in menus)
        {
            _appStorageService.Menus.TryAdd(menuBean.Id, menuBean);
        }
    }
}