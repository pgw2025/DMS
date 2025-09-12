using System.Windows;
using DMS.WPF.ViewModels.Dialogs;

namespace DMS.WPF.Views.Dialogs
{
    /// &lt;summary&gt;
    /// HistorySettingsDialog.xaml 的交互逻辑
    /// &lt;/summary&gt;
    public partial class HistorySettingsDialog 
    {
        public HistorySettingsDialog()
        {
            InitializeComponent();
        }
        
        public HistorySettingsDialog(HistorySettingsDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}