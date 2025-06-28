using CommunityToolkit.Mvvm.ComponentModel;

namespace PMSWPF.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public Object NavgateParameters { get; set; }
    public abstract void OnLoaded();
}