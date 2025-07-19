using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.Views.Dialogs;

public partial class ProcessingDialog : ContentDialog
{
    public ProcessingDialog(ProcessingDialogViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}