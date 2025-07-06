using System.Windows;
using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs;

public partial class DeviceDialog
{
    public DeviceDialog(DeviceDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Log the ProtocolType value
        if (viewModel.Device != null)
        {
            NlogHelper.Info($"DeviceDialog opened. Device ProtocolType: {viewModel.Device.ProtocolType}");
        }
        else
        {
            NlogHelper.Info("DeviceDialog opened. Device is null.");
        }
    }

}