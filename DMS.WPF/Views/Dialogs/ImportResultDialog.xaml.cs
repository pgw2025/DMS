using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.WPF.Views.Dialogs;

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
