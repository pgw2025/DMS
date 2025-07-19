using System.Windows.Controls;
using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.Views.Dialogs;

public partial class ConfirmDialog : ContentDialog
{
    
    public ConfirmDialog(ConfrimDialogViewModel viewModel)
    {
        InitializeComponent();
        this.DataContext = viewModel;
    }
}