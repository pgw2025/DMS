using System.Windows;
using DMS.Core.Helper;
using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.WPF.Views.Dialogs;

public partial class DeviceDialog
{
    public DeviceDialog()
    {
        InitializeComponent();

        // Log the ProtocolType value
        // if (viewModel.Device != null)
        // {
        //     NlogHelper.Info($"DeviceDialog opened. Device ProtocolType: {viewModel.Device.ProtocolType}");
        // }
        // else
        // {
        //     NlogHelper.Info("DeviceDialog opened. Device is null.");
        // }
    }

}