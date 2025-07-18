using System.Windows.Controls;
using DMS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.Views;

public partial class SettingView : UserControl
{
    public SettingView()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetRequiredService<SettingViewModel>();
    }
}