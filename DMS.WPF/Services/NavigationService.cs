using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels;
using DMS.WPF.ViewModels.Items;
using DMS.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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
    public async Task NavigateToAsync(object sender,NavigationParameter parameter)
    {
        if (parameter == null || string.IsNullOrWhiteSpace(parameter.TargetViewKey) )return;
        
        
        var viewModel = GetViewModelByKey(parameter.TargetViewKey);
        if (viewModel == null)
        {
            _notificationService.ShowError($"切换界面失败，没有找到界面：{parameter.TargetViewKey}");
            return;
        }
        
        if (sender is INavigatable fromViewModel)
        {
            await fromViewModel.OnNavigatedFromAsync(parameter);
        }

        var mainViewModel = App.Current.Services.GetRequiredService<MainViewModel>();
        mainViewModel.CurrentViewModel = viewModel;
        
        if (viewModel is INavigatable toViewModel)
        {
            await toViewModel.OnNavigatedToAsync(parameter);
        }
    }


    private ViewModelBase GetViewModelByKey(string key)
    {
        try
        {
            switch (key)
            {
                case nameof(HomeViewModel):
                    return App.Current.Services.GetRequiredService<HomeViewModel>();
                case nameof(DevicesViewModel):
                    return App.Current.Services.GetRequiredService<DevicesViewModel>();
                case nameof(DeviceDetailViewModel):
                    return App.Current.Services.GetRequiredService<DeviceDetailViewModel>();
                case nameof(DataTransformViewModel):
                    return App.Current.Services.GetRequiredService<DataTransformViewModel>();
                case nameof(VariableTableViewModel):
                    return App.Current.Services.GetRequiredService<VariableTableViewModel>();
                case nameof(VariableHistoryViewModel):
                    return App.Current.Services.GetRequiredService<VariableHistoryViewModel>();
                case nameof(LogHistoryViewModel):
                    return App.Current.Services.GetRequiredService<LogHistoryViewModel>();
                case nameof(MqttsViewModel):
                    return App.Current.Services.GetRequiredService<MqttsViewModel>();
                case nameof(MqttServerDetailViewModel):
                    return App.Current.Services.GetRequiredService<MqttServerDetailViewModel>();
                case nameof(SettingViewModel):
                    return App.Current.Services.GetRequiredService<SettingViewModel>();
                case nameof(EmailManagementViewModel):
                    return App.Current.Services.GetRequiredService<EmailManagementViewModel>();
                case nameof(TriggersViewModel):
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