using System.Windows;
using DMS.WPF.ViewModels.Dialogs;

namespace DMS.WPF.Views.Dialogs
{
    /// &lt;summary&gt;
    /// AlarmSettingsDialog.xaml 的交互逻辑
    /// &lt;/summary&gt;
    public partial class AlarmSettingsDialog 
    {
        public AlarmSettingsDialog()
        {
            InitializeComponent();
        }
        
        public AlarmSettingsDialog(AlarmSettingsDialogViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}