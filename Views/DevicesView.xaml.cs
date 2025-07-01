using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using PMSWPF.ViewModels;

namespace PMSWPF.Views;

public partial class DevicesView : UserControl
{
    public DevicesView()
    {
        InitializeComponent();
        DataContext=App.Current.Services.GetRequiredService<DevicesViewModel>();
    }
    

}