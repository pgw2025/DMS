using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs;

public partial class MqttAliasDialog : ContentDialog
{
    public MqttAliasDialog(MqttAliasDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}