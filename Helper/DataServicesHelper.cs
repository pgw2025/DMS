using Microsoft.Extensions.DependencyInjection;
using PMSWPF.Enums;
using PMSWPF.Models;
using PMSWPF.ViewModels;

namespace PMSWPF.Helper;

public class DataServicesHelper
{
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