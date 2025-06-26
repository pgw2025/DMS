using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PMSWPF.Data.Entities;
using PMSWPF.Data.Repositories;
using PMSWPF.Message;
using PMSWPF.Models;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly NavgatorServices _navgatorServices;

    [ObservableProperty] private ViewModelBase currentViewModel;
    [ObservableProperty]
    private ObservableCollection<MenuBean> _menus;

    private readonly MenuRepository _menuRepository;

    public MainViewModel(NavgatorServices navgatorServices)
    {
        _navgatorServices = navgatorServices;
        _menuRepository = new MenuRepository();
        _navgatorServices.OnViewModelChanged += () => { CurrentViewModel = _navgatorServices.CurrentViewModel; };
        CurrentViewModel = new HomeViewModel();
        CurrentViewModel.OnLoaded();
        
        WeakReferenceMessenger.Default.Register<UpdateMenuMessage>( this,UpdateMenu);
        
    }

    private async void UpdateMenu(object recipient, UpdateMenuMessage message)
    {
       await  LoadMenu();
    }


    public override async void OnLoaded()
    {
        await LoadMenu();
    }

    private async Task LoadMenu()
    {
        var menuList= await _menuRepository.GetMenu();
        Menus=new ObservableCollection<MenuBean>(menuList);
    }
}