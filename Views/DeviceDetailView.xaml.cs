using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using PMSWPF.ViewModels;

namespace PMSWPF.Views;

public partial class DeviceDetailView : UserControl
{
    public DeviceDetailView()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetRequiredService<DeviceDetailViewModel>();
    }
}