using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly NavgatorServices _navgatorServices;

    [ObservableProperty] private ViewModelBase currentViewModel;

    public MainViewModel(NavgatorServices navgatorServices)
    {
        _navgatorServices = navgatorServices;
        _navgatorServices.OnViewModelChanged += () => { CurrentViewModel = _navgatorServices.CurrentViewModel; };
        CurrentViewModel = new HomeViewModel();
    }


    public override void OnLoaded()
    {
    }
}