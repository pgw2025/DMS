using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Core.Enums;
using DMS.Core.Models;
using DMS.WPF.Helper;
using DMS.WPF.Interfaces;
using DMS.WPF.Services;
using DMS.WPF.ItemViewModel;
using DMS.WPF.Views;
using Microsoft.Extensions.Logging;

// AddAsync this using directive
// AddAsync this using directive

namespace DMS.WPF.ViewModels;

/// <summary>
/// 主视图模型，负责应用程序的主导航和数据管理。
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly IWPFDataService _wpfDataService;
    private readonly IDataStorageService _dataStorageService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<MainViewModel> _logger;

    /// <summary>
    /// 当前显示的视图模型。
    /// </summary>
    [ObservableProperty]
    private ViewModelBase _currentViewModel;

    /// <summary>
    /// 应用程序的菜单列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MenuItem> _menuTrees;

    /// <summary>
    /// 初始化 <see cref="MainViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="navgatorServices">导航服务。</param>
    /// <param name="dataServices">数据服务。</param>
    /// <param name="dialogService">对话框服务。</param>
    /// <param name="logger">日志记录器。</param>
    /// <param name="wpfDataService"></param>
    public MainViewModel(IWPFDataService wpfDataService ,IDataStorageService dataStorageService,INavigationService navigationService,
                         ILogger<MainViewModel> logger)
    {
        _wpfDataService = wpfDataService;
        _dataStorageService = dataStorageService;
        _navigationService = navigationService;
        _logger = logger;
        MenuTrees = _dataStorageService.MenuTrees;

        CurrentViewModel = new HomeViewModel();
        CurrentViewModel.OnLoaded();
    }

    /// <summary>
    /// 显示主窗口的命令。
    /// </summary>
    [RelayCommand]
    private void ShowWindow()
    {
        if (App.Current.MainWindow is MainView mainWindow)
        {
            mainWindow.ShowApplication();
        }
    }

    /// <summary>
    /// 退出应用程序的命令。
    /// </summary>
    [RelayCommand]
    private void ExitApplication()
    {
        // Application.Current.Shutdown();
    }



}