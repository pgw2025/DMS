using System.Collections.ObjectModel;
using AutoMapper;
using DMS.Application.Interfaces;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Services;

/// <summary>
/// 菜单数据服务类，负责管理菜单相关的数据和操作。
/// </summary>
public class MenuDataService : IMenuDataService
{
    private readonly IMapper _mapper;
    private readonly IDataStorageService _dataStorageService;
    private readonly IAppDataStorageService _appDataStorageService;



    /// <summary>
    /// MenuDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="appDataStorageService">数据服务中心实例。</param>
    public MenuDataService(IMapper mapper,IDataStorageService dataStorageService, IAppDataStorageService appDataStorageService)
    {
        _mapper = mapper;
        _dataStorageService = dataStorageService;
        _appDataStorageService = appDataStorageService;
    }

    public void LoadAllMenus()
    {
        _dataStorageService.Menus = _mapper.Map<ObservableCollection<MenuItemViewModel>>(_appDataStorageService.Menus.Values);
        BuildMenuTrees();
    }

    /// <summary>
    /// 构建菜单树。
    /// </summary>
    public void BuildMenuTrees()
    {
        _dataStorageService.MenuTrees.Clear();
        // 遍历所有菜单项，构建树形结构
        foreach (var menu in _dataStorageService.Menus)
        {
            var parentMenu = _dataStorageService.Menus.FirstOrDefault(m => m.Id == menu.ParentId);
            // 检查是否有父ID，并且父ID不为0（通常0或null表示根节点）
            if (parentMenu != null && menu.ParentId != 0)
            {
                // 将当前菜单添加到父菜单的Children列表中
                if (!parentMenu.Children.Contains(menu))
                {
                    parentMenu.Children.Add(menu);
                }
            }
            else
            {
                // 如果没有父ID，则这是一个根菜单
                _dataStorageService.MenuTrees.Add(menu);
            }
        }
    }

    /// <summary>
    /// 添加菜单项。
    /// </summary>
    public void AddMenuItem(MenuItemViewModel menuItemViewModel)
    {
        if (menuItemViewModel == null)
        {
            return;
        }

        var deviceMenu = _dataStorageService.Menus.FirstOrDefault(m => m.Id == menuItemViewModel.ParentId);
        if (deviceMenu != null)
        {
            deviceMenu.Children.Add(menuItemViewModel);
            _dataStorageService.Menus.Add(menuItemViewModel);
        }
    }


    /// <summary>
    /// 删除菜单项。
    /// </summary>
    public void DeleteMenuItem(MenuItemViewModel? menuItemViewModel)
    {
        if (menuItemViewModel == null)
        {
            return;
        }

        // 从扁平菜单列表中移除
        _dataStorageService.Menus.Remove(menuItemViewModel);

        // 从树形结构中移除
        if (menuItemViewModel.ParentId.HasValue && menuItemViewModel.ParentId.Value != 0)
        {
            // 如果有父菜单，从父菜单的Children中移除
            var parentMenu = _dataStorageService.Menus.FirstOrDefault(m => m.Id == menuItemViewModel.ParentId.Value);
            parentMenu?.Children.Remove(menuItemViewModel);
        }
        else
        {
            // 如果是根菜单，从MenuTrees中移除
            _dataStorageService.MenuTrees.Remove(menuItemViewModel);
        }
    }
}