using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Core.Models;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Core.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace DMS.Application.Services;

/// <summary>
/// 菜单管理服务，负责菜单相关的业务逻辑。
/// </summary>
public class MenuManagementService : IMenuManagementService
{
    private readonly IMenuService _menuService;
    private readonly IAppDataStorageService _appDataStorageService;

    /// <summary>
    /// 当菜单数据发生变化时触发
    /// </summary>
    public event EventHandler<MenuChangedEventArgs> MenuChanged;

    public MenuManagementService(IMenuService menuService,IAppDataStorageService appDataStorageService)
    {
        _menuService = menuService;
        _appDataStorageService = appDataStorageService;
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
        return await _menuService.CreateMenuAsync(menuDto);
    }

    /// <summary>
    /// 异步更新一个已存在的菜单。
    /// </summary>
    public async Task UpdateMenuAsync(MenuBeanDto menuDto)
    {
        await _menuService.UpdateMenuAsync(menuDto);
    }

    /// <summary>
    /// 异步删除一个菜单。
    /// </summary>
    public async Task DeleteMenuAsync(int id)
    {
        await _menuService.DeleteMenuAsync(id);
    }

    /// <summary>
    /// 在内存中添加菜单
    /// </summary>
    public void AddMenuToMemory(MenuBeanDto menuDto)
    {
        if (_appDataStorageService.Menus.TryAdd(menuDto.Id, menuDto))
        {
            MenuBeanDto parentMenu = null;
            if (menuDto.ParentId > 0 && _appDataStorageService.Menus.TryGetValue(menuDto.ParentId, out var parent))
            {
                parentMenu = parent;
                parent.Children.Add(menuDto);
            }

            OnMenuChanged(new MenuChangedEventArgs(DataChangeType.Added, menuDto, parentMenu));
        }
    }

    /// <summary>
    /// 在内存中更新菜单
    /// </summary>
    public void UpdateMenuInMemory(MenuBeanDto menuDto)
    {
        _appDataStorageService.Menus.AddOrUpdate(menuDto.Id, menuDto, (key, oldValue) => menuDto);

        MenuBeanDto parentMenu = null;
        if (menuDto.ParentId > 0 && _appDataStorageService.Menus.TryGetValue(menuDto.ParentId, out var parent))
        {
            parentMenu = parent;
        }

        OnMenuChanged(new MenuChangedEventArgs(DataChangeType.Updated, menuDto, parentMenu));
    }

    /// <summary>
    /// 在内存中删除菜单
    /// </summary>
    public void RemoveMenuFromMemory(int menuId)
    {
        if (_appDataStorageService.Menus.TryRemove(menuId, out var menuDto))
        {
            MenuBeanDto parentMenu = null;
            if (menuDto.ParentId > 0 && _appDataStorageService.Menus.TryGetValue(menuDto.ParentId, out var parent))
            {
                parentMenu = parent;
            }

            OnMenuChanged(new MenuChangedEventArgs(DataChangeType.Deleted, menuDto, parentMenu));
        }
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
        MenuChanged?.Invoke(this, e);
    }
}