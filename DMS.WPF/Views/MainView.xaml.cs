using System.Windows;
using DMS.Core.Helper;
using DMS.WPF.Models;
using DMS.WPF.Services;
using DMS.WPF.ViewModels;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using DMS.Core.Enums;

namespace DMS.WPF.Views;

/// <summary>
///     MainView.xaml 的交互逻辑
/// </summary>
// using Hardcodet.NotifyIcon.Wpf;

public partial class MainView : Window
{
    private readonly DataServices _dataServices;
    private MainViewModel _viewModel;

    public MainView(DataServices dataServices)
    {
        InitializeComponent();
        _viewModel = App.Current.Services.GetRequiredService<MainViewModel>();
        _dataServices = dataServices;
        DataContext = _viewModel;
        NlogHelper.Info("主界面加载成功");

        // Set the NotifyIcon's DataContext to the ViewModel
        MyNotifyIcon.DataContext = _viewModel;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        var settings = Config.ConnectionSettings.Load();
        if (settings.MinimizeToTrayOnClose)
        {
            // Hide the window instead of closing it
            e.Cancel = true;
            Hide();
        }
        else
        {
            Application.Current.Shutdown();
        }
    }

    public void ShowApplication()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
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