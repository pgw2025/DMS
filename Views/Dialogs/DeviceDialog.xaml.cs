using System.Windows;
using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.Models;
using PMSWPF.ViewModels.Dialogs;
using NLog; // Add NLog using directive

namespace PMSWPF.Views.Dialogs;

public partial class DeviceDialog
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger(); // Add Logger

    public DeviceDialog(DeviceDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Log the ProtocolType value
        if (viewModel.Device != null)
        {
            Logger.Info($"DeviceDialog opened. Device ProtocolType: {viewModel.Device.ProtocolType}");
        }
        else
        {
            Logger.Info("DeviceDialog opened. Device is null.");
        }
    }

}