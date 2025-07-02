using System.Windows;
using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.Models;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs;

public partial class DeviceDialog
{
    public DeviceDialog(DeviceDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

}