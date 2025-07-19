using CommunityToolkit.Mvvm.ComponentModel;

namespace DMS.WPF.ViewModels;

public abstract class ViewModelBase : ObservableObject
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



}