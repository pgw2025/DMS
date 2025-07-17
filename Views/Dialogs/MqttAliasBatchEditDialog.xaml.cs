using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs;

public partial class MqttAliasBatchEditDialog : ContentDialog
{
    public MqttAliasBatchEditDialog(MqttAliasBatchEditDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}