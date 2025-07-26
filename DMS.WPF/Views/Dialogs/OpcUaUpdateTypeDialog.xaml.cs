using DMS.WPF.ViewModels.Dialogs;

namespace DMS.WPF.Views.Dialogs
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
