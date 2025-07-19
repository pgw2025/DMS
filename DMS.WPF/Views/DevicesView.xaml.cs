using System.Windows.Controls;
using DMS.WPF.ViewModels;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.WPF.Views;

public partial class DevicesView : UserControl
{
    public DevicesView()
    {
        InitializeComponent();
        DataContext=App.Current.Services.GetRequiredService<DevicesViewModel>();
    }
    

}