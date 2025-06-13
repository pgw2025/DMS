using CommunityToolkit.Mvvm.ComponentModel;

namespace PMSWPF.ViewModels;

public abstract partial class ViewModelBase:ObservableObject
{
    public ViewModelBase()
    {
        
    }
    
    public abstract void OnLoaded();
}