using System.Windows;
using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.Models;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs;

public partial class DeviceDialog
{

    public DeviceDialog(DeviceDialogViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }

    private void OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
    }


    private void OnCloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var deferral = args.GetDeferral();
        deferral.Complete();
    }

    private async void TryOpenAnother(object sender, RoutedEventArgs e)
    {
        // try
        // {
        //     await new TestContentDialog { Owner = Owner }.ShowAsync();
        // }
        // catch (Exception ex)
        // {
        //     ErrorText.Text = ex.Message;
        //     ErrorText.Visibility = Visibility.Visible;
        // }
    }

    private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        ErrorText.Text = string.Empty;
        ErrorText.Visibility = Visibility.Collapsed;
    }
}

