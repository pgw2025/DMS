// 文件: DMS.WPF/Services/NavigationService.cs

using DMS.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using DMS.ViewModels;
using DMS.WPF.Views;

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
    public async Task NavigateToAsync(string viewKey, object parameter = null)
    {
        if (string.IsNullOrEmpty(viewKey))
        {
            return;
        }

        var mainViewModel = App.Current.Services.GetRequiredService<MainViewModel>();
        var viewModel = GetViewModelByKey(viewKey);

        if (viewModel is INavigatable navigatableViewModel)
        {
            await navigatableViewModel.OnNavigatedToAsync(parameter);
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
                throw new KeyNotFoundException($"未找到与键 '{key}' 关联的视图模型类型。请检查 NavigationService 的映射配置。");
        }
    }
}