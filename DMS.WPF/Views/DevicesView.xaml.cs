using System.Windows.Controls;
using DMS.ViewModels;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace DMS.Views;

public partial class DevicesView : UserControl
{
    public DevicesView()
    {
        InitializeComponent();
        DataContext=App.Current.Services.GetRequiredService<DevicesViewModel>();
    }
    

}