using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMSWPF.Helper;
using PMSWPF.Message;
using PMSWPF.Models;
using PMSWPF.ViewModels;

namespace PMSWPF.Views;

/// <summary>
///     MainView.xaml 的交互逻辑
/// </summary>
public partial class MainView : Window
{
    private readonly ILogger<MainView> _logger;
    private MainViewModel _viewModel;
    public MainView(MainViewModel viewModel, ILogger<MainView> logger)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _logger = logger;
        DataContext = viewModel;
        _logger.LogInformation("主界面加载成功");
    }

    /// <summary>
    ///     左边菜单项被点击的事件，切换右边的视图
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        var item = args.SelectedItem as NavigationViewItem;
        ViewModelBase navgateVM = App.Current.Services.GetRequiredService<HomeViewModel>();
        switch (item.Tag)
        {
            case "Home":
                // mainViewModel.NavgateTo<HomeViewModel>();
                navgateVM = App.Current.Services.GetRequiredService<HomeViewModel>();
                _logger.LogInformation("导航到到主页面");
                break;
            case "Devices":
                navgateVM = App.Current.Services.GetRequiredService<DevicesViewModel>();
                // mainViewModel.NavgateTo<DevicesViewModel>();
                _logger.LogInformation("导航到到设备页面");
                break;
            case "DataTransform":
                navgateVM = App.Current.Services.GetRequiredService<DataTransformViewModel>();
                // mainViewModel.NavgateTo<DataTransformViewModel>();
                _logger.LogInformation("导航到到数据转换页面");
                break;
            case "Setting":
                // mainViewModel.NavgateTo<SettingViewModel>();
                navgateVM = App.Current.Services.GetRequiredService<SettingViewModel>();
                _logger.LogInformation("导航到到设备页面");
                break;
        }
        
        MessageHelper.SendNavgatorMessage(navgateVM);
    }

    private async void MainView_OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnLoaded();
    }

    private void NavigationView_OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
        ViewModelBase navgateVM = App.Current.Services.GetRequiredService<HomeViewModel>();
        Object parameter =null;
        switch (args.InvokedItem)
        {
            case "主页":
                // mainViewModel.NavgateTo<HomeViewModel>();
                navgateVM = App.Current.Services.GetRequiredService<HomeViewModel>();
                _logger.LogInformation("导航到到主页面");
                break;
            case "设备":
                navgateVM = App.Current.Services.GetRequiredService<DevicesViewModel>();
                // mainViewModel.NavgateTo<DevicesViewModel>();
                _logger.LogInformation("导航到到设备页面");
                break;
            case "数据转换":
                navgateVM = App.Current.Services.GetRequiredService<DataTransformViewModel>();
                // mainViewModel.NavgateTo<DataTransformViewModel>();
                _logger.LogInformation("导航到到数据转换页面");
                break;
            case "设置":
                // mainViewModel.NavgateTo<SettingViewModel>();
                navgateVM = App.Current.Services.GetRequiredService<SettingViewModel>();
                _logger.LogInformation("导航到到设备页面");
                break;
        }
        
        MessageHelper.SendNavgatorMessage(navgateVM,parameter);
    }

    private void NavigationView_OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
      var selectMenu=  args.SelectedItem as MenuBean;

    }
}