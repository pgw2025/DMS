using Microsoft.Extensions.DependencyInjection;
using PMSWPF.Enums;
using PMSWPF.Models;
using PMSWPF.ViewModels;

namespace PMSWPF.Helper;

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
    public static MenuBean FindMenusForDevice(Device device,List<MenuBean> menus)
    {
        foreach (var mainMenu in menus)
        {
            if (mainMenu.Items == null || mainMenu.Items.Count == 0)
                continue;
            foreach (var secondMenu in mainMenu.Items)
            {
                if (secondMenu.Type == MenuType.DeviceMenu && secondMenu.Data != null && secondMenu.Data == device)
                {
                    return secondMenu;
                }
            }
        }
        return null;
    }
    
    /// <summary>
    /// 给菜单排序
    /// </summary>
    /// <param name="menu"></param>
    public static void SortMenus(MenuBean menu)
    {
        if (menu.Items == null || menu.Items.Count() == 0)
            return;
        menu.Items.Sort((a, b) =>
            a.Type.ToString().Length.CompareTo(b.Type.ToString().Length)
        );
        foreach (var menuItem in menu.Items)
        {
            SortMenus(menuItem);
        }
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
            case "数据转换":
                navgateVM = App.Current.Services.GetRequiredService<DataTransformViewModel>();
                break;
            case "设置":
                navgateVM = App.Current.Services.GetRequiredService<SettingViewModel>();
                break;
        }

        return navgateVM;
    }
}