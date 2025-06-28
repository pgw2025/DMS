using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PMSWPF.Data.Entities;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Message;
using PMSWPF.Models;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly NavgatorServices _navgatorServices;
    private readonly DataServices _dataServices;

    [ObservableProperty] private ViewModelBase currentViewModel;
    [ObservableProperty]
    private ObservableCollection<MenuBean> _menus;

    private readonly MenuRepository _menuRepository;

    public MainViewModel(NavgatorServices navgatorServices,DataServices dataServices)
    {
        _navgatorServices = navgatorServices;
        _dataServices = dataServices;

        _navgatorServices.OnViewModelChanged += () => { CurrentViewModel = _navgatorServices.CurrentViewModel; };
        CurrentViewModel = new HomeViewModel();
        CurrentViewModel.OnLoaded();
        
        WeakReferenceMessenger.Default.Send<LoadMessage>(new LoadMessage(LoadTypes.Menu));
        dataServices.OnMenuListChanged += (menus) =>
        {
            Menus = new ObservableCollection<MenuBean>(menus);
        };

    }


    public override void OnLoaded()
    {
        
    }
}