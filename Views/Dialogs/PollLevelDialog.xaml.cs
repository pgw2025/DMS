using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs
{
    public partial class PollLevelDialog : ContentDialog
    {
        public PollLevelDialog(PollLevelDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
