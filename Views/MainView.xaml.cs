using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.ViewModels;

namespace PMSWPF.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {

        public MainView()
        {

            InitializeComponent();
            
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            NavigationViewItem? item = args.SelectedItem as NavigationViewItem;
            MainViewModel mainViewModel = (MainViewModel)this.DataContext ;
            switch (item.Tag)
            {
                case "Home":
                    mainViewModel.NavgateTo<HomeViewModel>();
                    break;
                case "Devices":
                    mainViewModel.NavgateTo<DevicesViewModel>();
                    break;
                case "DataTransform":
                    mainViewModel.NavgateTo<DataTransformViewModel>();
                    break;
                default:
                    mainViewModel.NavgateTo<HomeViewModel>();
                    break;
                
            }
        }
    }
}
