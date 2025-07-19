using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.Views.Dialogs
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
