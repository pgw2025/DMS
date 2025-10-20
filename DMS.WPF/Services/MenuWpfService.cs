using AutoMapper;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Management;
using DMS.Application.Services.Management;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DMS.WPF.Services;

/// <summary>
/// 菜单数据服务类，负责管理菜单相关的数据和操作。
/// </summary>
public class MenuWpfService : IMenuWpfService
{
    private readonly IMapper _mapper;
    private readonly IWpfDataService _wpfDataService;
    private readonly IAppStorageService _appStorageService;
    private readonly IMenuManagementService _menuManagementService;



    /// <summary>
    /// MenuDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="appStorageService">数据服务中心实例。</param>
    public MenuWpfService(IMapper mapper, IWpfDataService dataStorageService, IAppStorageService appStorageService, IMenuManagementService menuManagementService)
    {
        _mapper = mapper;
        _wpfDataService = dataStorageService;
        _appStorageService = appStorageService;
        _menuManagementService = menuManagementService;
    }

    public void LoadAllMenus()
    {
        _wpfDataService.Menus = _mapper.Map<ObservableCollection<MenuItem>>(_appStorageService.Menus.Values);
        BuildMenuTrees();
    }

    /// <summary>
    /// 构建菜单树。
    /// </summary>
    public void BuildMenuTrees()
    {
        _wpfDataService.MenuTrees.Clear();
        // 遍历所有菜单项，构建树形结构
        foreach (var menu in _wpfDataService.Menus)
        {
            var parentMenu = _wpfDataService.Menus.FirstOrDefault(m => m.Id == menu.ParentId);
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
                _wpfDataService.MenuTrees.Add(menu);
            }
        }
    }

    /// <summary>
    /// 添加菜单项。
    /// </summary>
    public void AddMenuToView(MenuItem MenuItem)
    {
        if (MenuItem is null) return;

        var deviceMenu = _wpfDataService.Menus.FirstOrDefault(m => m.Id == MenuItem.ParentId);
        if (deviceMenu is not null)
        {

            deviceMenu.Children.Add(MenuItem);
            _wpfDataService.Menus.Add(MenuItem);
            BuildMenuTrees();

        }
    }

    /// <summary>
    /// 更新菜单项。
    /// </summary>
    public async Task UpdateMenuItem(MenuItem MenuItem)
    {
        if (MenuItem is null) return;

        var menu = _wpfDataService.Menus.FirstOrDefault(m => m.Id == MenuItem.Id);
        if (menu is not null)
        {

            var res = await _menuManagementService.UpdateMenuAsync(_mapper.Map<MenuBean>(MenuItem));
            if (res > 0)
            {
                menu.Header = MenuItem.Header;
            }

        }
    }


    /// <summary>
    /// 删除菜单项。
    /// </summary>
    public async Task DeleteMenuItem(MenuItem? MenuItem)
    {
        if (MenuItem is null) return;

        await _menuManagementService.DeleteMenuAsync(MenuItem.Id);

        // 从扁平菜单列表中移除
        _wpfDataService.Menus.Remove(MenuItem);

        //// 从树形结构中移除
        if (MenuItem.ParentId.HasValue && MenuItem.ParentId.Value != 0)
        {
            // 如果有父菜单，从父菜单的Children中移除
            var parentMenu = _wpfDataService.Menus.FirstOrDefault(m => m.Id == MenuItem.ParentId.Value);
            parentMenu?.Children.Remove(MenuItem);
        }
        else
        {
            // 如果是根菜单，从MenuTrees中移除
            _wpfDataService.MenuTrees.Remove(MenuItem);
        }

        //BuildMenuTrees();
    }
}