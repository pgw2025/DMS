using System.ComponentModel;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PMSWPF.ViewModels;

namespace PMSWPF.Services;

public class NavgatorServices
{
    private ViewModelBase currentViewModel;

    public ViewModelBase CurrentViewModel
    {
        get { return currentViewModel; }
        set
        {
            currentViewModel = value; 
            OnViewModelChanged?.Invoke();
        }
    }

    public event Action OnViewModelChanged ;

    public void NavigateTo<T>() where T : ViewModelBase
    {
        // Application.Current
        CurrentViewModel = App.Current.Services.GetService<T>();
    }
}