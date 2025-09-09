using System.Collections.ObjectModel;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Interfaces;

/// <summary>
/// 菜单数据服务接口。
/// </summary>
public interface IMenuDataService
{

    /// <summary>
    /// 构建菜单树。
    /// </summary>
    void BuildMenuTrees();

    /// <summary>
    /// 添加菜单项。
    /// </summary>
    void AddMenuItem(MenuItemViewModel menuItemViewModel);

    /// <summary>
    /// 删除菜单项。
    /// </summary>
    void DeleteMenuItem(MenuItemViewModel? menuItemViewModel);

    void LoadAllMenus();

}