using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.Logging;
using PMSWPF.Message;
using PMSWPF.ViewModels;

namespace PMSWPF.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        private readonly ILogger<MainView> _logger;

        public MainView(MainViewModel viewModel, ILogger<MainView> logger)
        {
            
            _logger = logger;
            InitializeComponent();
            this.DataContext=viewModel;
            _logger.LogInformation("主界面加载成功");
            
          
        }

        /// <summary>
        /// 左边菜单项被点击的事件，切换右边的视图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem? item = args.SelectedItem as NavigationViewItem;
            MainViewModel mainViewModel = (MainViewModel)this.DataContext ;
            switch (item.Tag)
            {
                case "Home":
                    mainViewModel.NavgateTo<HomeViewModel>();
                    _logger.LogInformation("导航到到主页面");
                    break;
                case "Devices":
                   
                    mainViewModel.NavgateTo<DevicesViewModel>();
                    _logger.LogInformation("导航到到设备页面");
                    break;
                case "DataTransform":
                    mainViewModel.NavgateTo<DataTransformViewModel>();
                    _logger.LogInformation("导航到到数据转换页面");
                    break;
                case "Setting":
                    mainViewModel.NavgateTo<SettingViewModel>();
                    _logger.LogInformation("导航到到设备页面");
                    break;
                default:
                    mainViewModel.NavgateTo<HomeViewModel>();
                    _logger.LogInformation("没有设置Tag,默认导航到主页面。");
                    break;
                
            }
        }
    }
}
