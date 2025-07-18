using DMS.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.Views.Dialogs
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
