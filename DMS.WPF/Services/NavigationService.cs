using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels;
using DMS.WPF.ViewModels.Items;
using DMS.WPF.ViewModels.Triggers;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.WPF.Services;

/// <summary>
/// INavigationService 的实现，负责解析ViewModel并处理参数传递。
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// 构造函数。
    /// </summary>
    public NavigationService(IServiceProvider serviceProvider,INotificationService notificationService)
    {
        _serviceProvider = serviceProvider;
        _notificationService = notificationService;
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
            _notificationService.ShowError($"切换界面失败，没有找到界面：{menu.TargetViewKey}");
            return;
        }


        if (viewModel is INavigatable navigatableViewModel)
        {
            await navigatableViewModel.OnNavigatedToAsync(menu);
        }

        mainViewModel.CurrentViewModel = viewModel;
    }
    
    /// <summary>
    /// 导航到指定键的视图，并传递参数。
    /// </summary>
    public async Task NavigateToAsync(string viewKey, object parameter = null)
    {
        var mainViewModel = App.Current.Services.GetRequiredService<MainViewModel>();
        var viewModel = GetViewModelByKey(viewKey);
        if (viewModel == null)
        {
            _notificationService.ShowError($"切换界面失败，没有找到界面：{viewKey}");
            return;
        }

      

        mainViewModel.CurrentViewModel = viewModel;
    }


    private ViewModelBase GetViewModelByKey(string key)
    {
        try
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
                case "VariableHistoryView":
                    return App.Current.Services.GetRequiredService<VariableHistoryViewModel>();
                case "LogHistoryView":
                    return App.Current.Services.GetRequiredService<LogHistoryViewModel>();
                case "MqttsView":
                    return App.Current.Services.GetRequiredService<MqttsViewModel>();
                case "MqttServerDetailView":
                    return App.Current.Services.GetRequiredService<MqttServerDetailViewModel>();
                case "SettingView":
                    return App.Current.Services.GetRequiredService<SettingViewModel>();
                case "EmailManagementView":
                    return App.Current.Services.GetRequiredService<EmailManagementViewModel>();
                case "TriggersView":
                    return App.Current.Services.GetRequiredService<TriggersViewModel>();
                default:
                    return null;
            }
        }
        catch (Exception e)
        {
            _notificationService.ShowError($"切换界面失败，获取：{key}对应的ViewModel时发生了错误：{e.Message}");
            throw;
        }
    }
}