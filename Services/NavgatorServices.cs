using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PMSWPF.Message;
using PMSWPF.ViewModels;

namespace PMSWPF.Services;

public class NavgatorServices : ObservableRecipient, IRecipient<NavgatorMessage>
{
    private ViewModelBase currentViewModel;

    public NavgatorServices()
    {
        IsActive = true;
    }

    public ViewModelBase CurrentViewModel
    {
        get => currentViewModel;
        set
        {
            currentViewModel = value;
            OnViewModelChanged?.Invoke();
            currentViewModel.OnLoaded();
        }
    }

    public void Receive(NavgatorMessage message)
    {
        CurrentViewModel = message.Value;
    }

    public event Action OnViewModelChanged;

    public void NavigateTo<T>() where T : ViewModelBase
    {
        // Application.Current
        CurrentViewModel = App.Current.Services.GetService<T>();
    }
}