using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.WPF.Views.Dialogs;

public partial class MqttDialog : ContentDialog
{
    public MqttDialog(MqttDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}