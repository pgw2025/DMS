using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs;

public partial class ProcessingDialog : ContentDialog
{
    public ProcessingDialog(ProcessingDialogViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}