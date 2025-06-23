using CommunityToolkit.Mvvm.ComponentModel;

namespace PMSWPF.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public abstract void OnLoaded();
}