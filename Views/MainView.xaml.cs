using System.Windows;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMSWPF.Enums;
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
        try
        {
            var menu = args.SelectedItem as MenuBean;
            if (menu == null)
                throw new ArgumentException("选择的菜单项为空！");

            switch (menu.Type)
            {
                case MenuType.MainMenu:
                case MenuType.DeviceMenu:
                case MenuType.VariableTableMenu:
                    if (menu.ViewModel == null)
                        throw new ArgumentException($"菜单项：{menu.Name}，没有绑定对象的ViewModel");
                    MessageHelper.SendNavgatorMessage(menu.ViewModel);
                    _logger.LogInformation($"导航到：{menu.Name}");
                    break;
                case MenuType.AddVariableTableMenu:
                    break;
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowMessage(e.Message, NotificationType.Error);
            _logger.LogError(e.Message);
        }
    }

    private async void MainView_OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OnLoaded();
    }

}