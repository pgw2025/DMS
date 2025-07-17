using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs;

/// <summary>
/// ImportResultDialog.xaml 的交互逻辑
/// </summary>
public partial class ImportResultDialog : ContentDialog
{
    public ImportResultDialog()
    {
        InitializeComponent();
    }

    public ImportResultDialog(ImportResultDialogViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
