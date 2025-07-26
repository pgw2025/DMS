using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.WPF.Views.Dialogs;

public partial class VarTableDialog : ContentDialog
{
    public VarTableDialog(VarTableDialogViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}