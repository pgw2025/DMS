using System.Windows;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.Services;
using PMSWPF.ViewModels;

namespace PMSWPF.Views;

/// <summary>
///     MainView.xaml 的交互逻辑
/// </summary>
public partial class MainView : Window
{
    private readonly DataServices _dataServices;
    private readonly ILogger<MainView> _logger;
    private MainViewModel _viewModel;

    public MainView(DataServices dataServices, ILogger<MainView> logger)
    {
        InitializeComponent();
        _viewModel = App.Current.Services.GetRequiredService<MainViewModel>();
        _dataServices = dataServices;
        _logger = logger;
        DataContext = _viewModel;
        _logger.LogInformation("主界面加载成功");
    }

    /// <summary>
    ///     左边菜单项被点击的事件，切换右边的视图
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var menu = args.SelectedItem as MenuBean;
        if (menu != null)
        {
           await _viewModel.MenuSelectionChanged(menu);
        }
        else
        {
            NotificationHelper.ShowError("选择的菜单项为空！");
        }
    }

    private async void MainView_OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnLoaded();
    }
}