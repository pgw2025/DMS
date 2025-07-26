// 文件: DMS.WPF/Views/SplashWindow.xaml.cs
using DMS.WPF.ViewModels;
using System.Windows;

namespace DMS.WPF.Views;

/// <summary>
/// SplashWindow.xaml 的交互逻辑
/// </summary>
public partial class SplashWindow : Window
{
    public SplashWindow(SplashViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (s, e) =>
        {
            var success = await viewModel.InitializeAsync();
            if (success)
            {
                Close();
            }
        };
    }
}
