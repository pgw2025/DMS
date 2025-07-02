using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Message;
using PMSWPF.ViewModels;

namespace PMSWPF.Services;

public partial class NavgatorServices : ObservableRecipient, IRecipient<NavgatorMessage>
{
    private readonly ILogger<NavgatorServices> _logger;

    [ObservableProperty] private ViewModelBase currentViewModel;

    public NavgatorServices(ILogger<NavgatorServices> logger)
    {
        _logger = logger;
        IsActive = true;
    }

    partial void OnCurrentViewModelChanging(ViewModelBase viewModel)
    {
        viewModel?.OnLoading();
    }

    partial void OnCurrentViewModelChanged(ViewModelBase viewModel)
    {
        OnViewModelChanged?.Invoke();
        viewModel?.OnLoaded();
    }

    // public ViewModelBase CurrentViewModel
    // {
    //     get => currentViewModel;
    //     set
    //     {
    //         value?.OnLoading();
    //         currentViewModel = value;
    //         OnViewModelChanged?.Invoke();
    //         currentViewModel?.OnLoaded();
    //     }
    // }


    public void Receive(NavgatorMessage message)
    {
        try
        {
            CurrentViewModel = message.Value;
        }
        catch (Exception e)
        {
            NotificationHelper.ShowMessage($"切换视图时发生了错误：{e.Message}", NotificationType.Error);
            _logger.LogError($"切换视图时发生了错误：{e}");
        }
    }

    public event Action OnViewModelChanged;
}