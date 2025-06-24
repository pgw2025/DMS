using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Data.Entities;
using PMSWPF.Data.Repositories;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly NavgatorServices _navgatorServices;

    [ObservableProperty] private ViewModelBase currentViewModel;
    [ObservableProperty]
    private ObservableCollection<DbMenu> _menus;

    public MainViewModel(NavgatorServices navgatorServices)
    {
        _navgatorServices = navgatorServices;
        _navgatorServices.OnViewModelChanged += () => { CurrentViewModel = _navgatorServices.CurrentViewModel; };
        CurrentViewModel = new HomeViewModel();
        CurrentViewModel.OnLoaded();
    }


    public override async void OnLoaded()
    {
        MenuRepositories mr = new MenuRepositories();
       var menuList= await mr.GetMenu();
       Menus=new ObservableCollection<DbMenu>(menuList);
    }
}