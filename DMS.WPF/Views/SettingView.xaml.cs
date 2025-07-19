using System.Windows.Controls;
using DMS.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.WPF.Views;

public partial class SettingView : UserControl
{
    public SettingView()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetRequiredService<SettingViewModel>();
    }
}