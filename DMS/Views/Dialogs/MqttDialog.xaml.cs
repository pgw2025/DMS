using System.Windows.Controls;
using DMS.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.Views.Dialogs;

using DMS.ViewModels.Dialogs;

public partial class MqttDialog : ContentDialog
{
    public MqttDialog(MqttDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}