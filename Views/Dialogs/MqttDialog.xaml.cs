using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Controls;

namespace PMSWPF.Views.Dialogs;

using PMSWPF.ViewModels.Dialogs;

public partial class MqttDialog : ContentDialog
{
    public MqttDialog(MqttDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}