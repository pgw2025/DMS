using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.ViewModels;

public abstract class ViewModelBase : ObservableObject,INavigatable
{
    public virtual void OnLoaded()
    {
        
    }

    public virtual void OnLoading()
    {
        
    }
    
    public virtual async Task<bool> OnExitAsync()
    {
        return true;
    }


    public virtual async Task OnNavigatedToAsync(NavigationParameter parameter)
    {
        
    }

    public virtual async Task OnNavigatedFromAsync(NavigationParameter parameter)
    {
        
    }
}