
using DMS.Core.Enums;
using DMS.Core.Models;
using DMS.WPF.ViewModels;
using DMS.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.WPF.Helper;

public class DataServicesHelper
{
    
    /// <summary>
    /// 从设备列表中找到变量表VarTable对象
    /// </summary>
    /// <param name="vtableId">VarTable的ID</param>
    /// <returns>如果找到择返回对象，否则返回null</returns>
    public static VariableTable FindVarTableForDevice(List<Device> devices, int vtableId)
    {
        VariableTable varTable = null;
        foreach (var device in devices)
        {
            varTable = device.VariableTables.FirstOrDefault(v => v.Id == vtableId);
            if (varTable != null)
                return varTable;
        }

        return varTable;
    }
    
    
    
    
    public static MenuBean FindMenusForDevice(Device device, IEnumerable<MenuBean> menus)
    {
        // if (menus == null)
        // {
        //     return null;
        // }
        //
        // foreach (var menu in menus)
        // {
        //     // 检查当前菜单项是否匹配
        //     if (menu.Type==MenuType.DeviceMenu && menu.DataId ==device.Id)
        //     {
        //         return menu;
        //     }
        //
        //     // 递归搜索子菜单
        //     var foundInSubMenu = FindMenusForDevice(device, menu.Items);
        //     if (foundInSubMenu != null)
        //     {
        //         return foundInSubMenu;
        //     }
        // }

        return null;
    }
    
    /// <summary>
    /// 给菜单排序
    /// </summary>
    /// <param name="menu"></param>
    public static void SortMenus(MenuBean menu)
    {
        // if (menu.Items == null || menu.Items.Count() == 0)
        //     return;
        // menu.Items.Sort((a, b) =>
        //     a.Type.ToString().Length.CompareTo(b.Type.ToString().Length)
        // );
        // foreach (var menuItem in menu.Items)
        // {
        //     SortMenus(menuItem);
        // }
    }
    
    public static ViewModelBase GetMainViewModel(string name)
    {
        ViewModelBase navgateVM = App.Current.Services.GetRequiredService<HomeViewModel>();
        switch (name)
        {
            case "主页":
                navgateVM = App.Current.Services.GetRequiredService<HomeViewModel>();
                break;
            case "设备":
                navgateVM = App.Current.Services.GetRequiredService<DevicesViewModel>();
                break;
            case "Mqtt服务器":
                navgateVM = App.Current.Services.GetRequiredService<MqttsViewModel>();
                break;
            case "数据转换":
                navgateVM = App.Current.Services.GetRequiredService<DataTransformViewModel>();
                break;
            case "设置":
                navgateVM = App.Current.Services.GetRequiredService<SettingViewModel>();
                break;
        }

        return navgateVM;
    }

    public static MenuBean FindVarTableMenu(int varTableId, List<MenuBean> menus)
    {
        // if (menus == null)
        // {
        //     return null;
        // }
        //
        // foreach (var menu in menus)
        // {
        //     // 检查当前菜单项是否匹配
        //     if (menu.Type==MenuType.VariableTableMenu && menu.DataId ==varTableId)
        //     {
        //         return menu;
        //     }
        //
        //     // 递归搜索子菜单
        //     var foundInSubMenu = FindVarTableMenu(varTableId, menu.Items);
        //     if (foundInSubMenu != null)
        //     {
        //         return foundInSubMenu;
        //     }
        // }

        return null;
    }
}
