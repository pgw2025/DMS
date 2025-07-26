using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.WPF.Views.Dialogs;

public partial class MqttAliasBatchEditDialog : ContentDialog
{
    public MqttAliasBatchEditDialog(MqttAliasBatchEditDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}