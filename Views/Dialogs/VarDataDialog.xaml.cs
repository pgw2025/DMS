using PMSWPF.ViewModels.Dialogs;
using System.Windows.Controls;

namespace PMSWPF.Views.Dialogs;

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