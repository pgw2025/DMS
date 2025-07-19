using System.Windows.Controls;
using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.Views.Dialogs;

using DMS.WPF.ViewModels.Dialogs;

public partial class MqttDialog : ContentDialog
{
    public MqttDialog(MqttDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}