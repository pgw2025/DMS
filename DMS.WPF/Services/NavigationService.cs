// 文件: DMS.WPF/Services/NavigationService.cs

using DMS.Helper;
using DMS.ViewModels;
using DMS.WPF.ViewModels;
using DMS.WPF.ViewModels.Items;
using DMS.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using DMS.WPF.Interfaces;

namespace DMS.WPF.Services;

/// <summary>
/// INavigationService 的实现，负责解析ViewModel并处理参数传递。
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// 构造函数。
    /// </summary>
    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// 导航到指定键的视图，并传递参数。
    /// </summary>
    public async Task NavigateToAsync(MenuItemViewModel menu)
    {
        if (string.IsNullOrEmpty(menu.TargetViewKey))
        {
            return;
        }

        var mainViewModel = App.Current.Services.GetRequiredService<MainViewModel>();
        var viewModel = GetViewModelByKey(menu.TargetViewKey);
        if (viewModel == null)
        {

            NotificationHelper.ShowError($"切换界面失败，没有找到界面：{menu.TargetViewKey}");
            return;
        }


        if (viewModel is INavigatable navigatableViewModel)
        {
            await navigatableViewModel.OnNavigatedToAsync(menu);
        }

        mainViewModel.CurrentViewModel = viewModel;
    }


    private ViewModelBase GetViewModelByKey(string key)
    {
        switch (key)
        {
            case "HomeView":
                return App.Current.Services.GetRequiredService<HomeViewModel>();
            case "DevicesView":
                return App.Current.Services.GetRequiredService<DevicesViewModel>();
            case "DeviceDetailView":
                return App.Current.Services.GetRequiredService<DeviceDetailViewModel>();
            case "DataTransformView":
                return App.Current.Services.GetRequiredService<DataTransformViewModel>();
            case "VariableTableView":
                return App.Current.Services.GetRequiredService<VariableTableViewModel>();
            case "MqttsView":
                return App.Current.Services.GetRequiredService<MqttsViewModel>();
            case "MqttServerDetailView":
                return App.Current.Services.GetRequiredService<MqttServerDetailViewModel>();
            case "SettingView":
                return App.Current.Services.GetRequiredService<SettingViewModel>();
            default:
                return null;
        }
    }
}