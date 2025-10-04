using System.Windows;
using DMS.WPF.Services;
using DMS.WPF.ViewModels;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using DMS.Core.Enums;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Views;

/// <summary>
///     MainView.xaml 的交互逻辑
/// </summary>
// using Hardcodet.NotifyIcon.Wpf;

public partial class MainView : Window
{
    private MainViewModel _viewModel;

    public MainView()
    {
        InitializeComponent();
        _viewModel = App.Current.Services.GetRequiredService<MainViewModel>();
        DataContext = _viewModel;
        
        // Set the NotifyIcon's DataContext to the ViewModel
        MyNotifyIcon.DataContext = _viewModel;
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
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
        var menu = args.SelectedItem as MenuItemViewModel;
        if (menu != null)
        {

            NavigationType navigationType = NavigationType.None;
            switch (menu.MenuType)
            {
                case MenuType.DeviceMenu:
                    navigationType=NavigationType.Device;
                    break;
                case MenuType.VariableTableMenu:
                    navigationType=NavigationType.VariableTable;
                    break;
                case MenuType.MqttMenu:
                    navigationType=NavigationType.Mqtt;
                    break;
                
            }
            
          var navigationService=  App.Current.Services.GetRequiredService<INavigationService>();
          navigationService.NavigateToAsync(this,new NavigationParameter(menu.TargetViewKey,menu.TargetId,navigationType));
        }
       
    }

    private async void MainView_OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnLoaded();
    }
}