using PMSWPF.ViewModels;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace PMSWPF.Views;

public partial class SettingView : UserControl
{
    public SettingView()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetRequiredService<SettingViewModel>();
    }
}