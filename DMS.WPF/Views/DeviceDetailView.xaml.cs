using System.Windows.Controls;
using DMS.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.WPF.Views;

public partial class DeviceDetailView : UserControl
{
    public DeviceDetailView()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetRequiredService<DeviceDetailViewModel>();
    }
}