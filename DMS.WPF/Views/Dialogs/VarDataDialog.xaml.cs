using DMS.WPF.ViewModels.Dialogs;

namespace DMS.WPF.Views.Dialogs;

public partial class VarDataDialog 
{
    public VarDataDialog()
    {
        InitializeComponent();
    }

    public VarDataDialog(VarDataDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}