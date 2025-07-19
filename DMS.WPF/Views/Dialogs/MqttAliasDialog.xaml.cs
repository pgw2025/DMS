using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.Views.Dialogs;

public partial class MqttAliasDialog : ContentDialog
{
    public MqttAliasDialog(MqttAliasDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}