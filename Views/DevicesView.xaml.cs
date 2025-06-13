using System.Windows;
using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.ViewModels;

namespace PMSWPF.Views;

public partial class DevicesView : UserControl
{
    public DevicesView()
    {
        InitializeComponent();
    }

    private void BasicGridView_ItemClick(object sender, ItemClickEventArgs e)
    {
        
    }

    private async void DevicesView_OnLoaded(object sender, RoutedEventArgs e)
    {
       //  var devicesViewModel = (DevicesViewModel)this.DataContext;
       // await  devicesViewModel.OnLoadedAsync();
    }
}