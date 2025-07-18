using System.Windows.Controls;
using DMS.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.Views;

public partial class DeviceDetailView : UserControl
{
    public DeviceDetailView()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetRequiredService<DeviceDetailViewModel>();
    }
}