using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs;

public partial class VarTableDialog : ContentDialog
{
    public VarTableDialog(VarTableDialogViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}