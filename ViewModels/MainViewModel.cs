using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PMSWPF.Data.Entities;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Helper;
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
        // 发送消息加载数据
        MessageHelper.SendLoadMessage(LoadTypes.All);
        // 当菜单加载成功后，在前台显示菜单
        dataServices.OnMenuListChanged += (menus) =>
        {
            Menus = new ObservableCollection<MenuBean>(menus);
        };

    }


    public override void OnLoaded()
    {
        
    }
}