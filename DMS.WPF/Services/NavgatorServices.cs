using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DMS.Helper;
using DMS.Message;
using DMS.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DMS.Core.Enums;

namespace DMS.Services;

public partial class NavgatorServices : ObservableRecipient, IRecipient<NavgatorMessage>
{
    // [ObservableProperty]
    private ViewModelBase currentViewModel;

    public NavgatorServices()
    {
        IsActive = true;
    }

    // partial void OnCurrentViewModelChanging(ViewModelBase viewModel)
    // {
    //     viewModel?.OnLoading();
    // }
    //
    // partial void OnCurrentViewModelChanged(ViewModelBase viewModel)
    // {
    //     OnViewModelChanged?.Invoke();
    //     viewModel?.OnLoaded();
    // }

    public ViewModelBase CurrentViewModel
    {
        get => currentViewModel;
        set { currentViewModel = value; }
    }


    public async void Receive(NavgatorMessage message)
    {
        try
        {
            ViewModelBase nextViewModel = message.Value;
            //如果OnExit返回False终止跳转

            if (currentViewModel != null)
            {
                var isExit = await currentViewModel.OnExitAsync();
                if (!isExit)
                {
                    return;
                }
            }

            nextViewModel?.OnLoading();
            CurrentViewModel = message.Value;
            OnViewModelChanged?.Invoke();
            currentViewModel?.OnLoaded();
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"切换视图时发生了错误：{e.Message}", e);
        }
    }

    public event Action OnViewModelChanged;
}