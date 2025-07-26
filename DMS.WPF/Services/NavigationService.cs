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

        var mainViewModel = _serviceProvider.GetRequiredService<MainViewModel>();
        var viewModelType = GetViewModelTypeByKey(viewKey);
        var viewModel = _serviceProvider.GetRequiredService(viewModelType) as ViewModelBase;

        if (viewModel is INavigatable navigatableViewModel)
        {
            await navigatableViewModel.OnNavigatedToAsync(parameter);
        }

        mainViewModel.CurrentViewModel = viewModel;
    }



    private Type GetViewModelTypeByKey(string key)
    {
        return key switch
               {
                   "HomeView" => typeof(HomeViewModel),
                   "DevicesView" => typeof(DevicesViewModel),
                   "DeviceDetailView" => typeof(DeviceDetailViewModel),
                   "VariableTableView" => typeof(VariableTableViewModel),
                   "MqttsView" => typeof(MqttsViewModel),
                   "MqttServerDetailView" => typeof(MqttServerDetailViewModel),
                   "SettingView" => typeof(SettingViewModel),
                   _ => throw new KeyNotFoundException($"未找到与键 '{key}' 关联的视图模型类型。请检查 NavigationService 的映射配置。")
               };
    }
}