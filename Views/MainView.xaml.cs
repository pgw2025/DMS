using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMSWPF.Message;
using PMSWPF.ViewModels;

namespace PMSWPF.Views;

/// <summary>
///     MainView.xaml 的交互逻辑
/// </summary>
public partial class MainView : Window
{
    private readonly ILogger<MainView> _logger;

    public MainView(MainViewModel viewModel, ILogger<MainView> logger)
    {
        _logger = logger;
        InitializeComponent();
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

        var nm = new NavgatorMessage(navgateVM);
        WeakReferenceMessenger.Default.Send(nm);
    }
}