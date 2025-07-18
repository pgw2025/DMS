using DMS.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.Views.Dialogs;

public partial class VarTableDialog : ContentDialog
{
    public VarTableDialog(VarTableDialogViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}