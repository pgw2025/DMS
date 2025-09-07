using System.Windows;
using System.Windows.Controls;
using DMS.WPF.ViewModels;

namespace DMS.WPF.Views;

/// <summary>
/// LogHistoryView.xaml 的交互逻辑
/// </summary>
public partial class LogHistoryView : UserControl
{
    public LogHistoryView()
    {
        InitializeComponent();
    }

    private void LogHistoryView_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is LogHistoryViewModel viewModel)
        {
            viewModel.OnLoaded();
        }
    }
}