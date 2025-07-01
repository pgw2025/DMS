using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs;

public partial class ConfirmDialog : ContentDialog
{
    
    public ConfirmDialog(ConfrimDialogViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}