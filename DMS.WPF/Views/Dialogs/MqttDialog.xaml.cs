using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.WPF.Views.Dialogs;

public partial class MqttDialog : ContentDialog
{
    public MqttDialog()
    {
        InitializeComponent();
    }

    public MqttDialog(MqttDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}