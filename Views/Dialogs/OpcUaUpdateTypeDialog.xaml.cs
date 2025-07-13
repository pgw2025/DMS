using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs
{
    public partial class OpcUaUpdateTypeDialog 
    {
        public OpcUaUpdateTypeDialog(OpcUaUpdateTypeDialogViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
