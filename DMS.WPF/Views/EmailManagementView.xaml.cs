using DMS.WPF.ViewModels;
using System.Windows.Controls;

namespace DMS.WPF.Views
{
    /// <summary>
    /// EmailManagementView.xaml 的交互逻辑
    /// </summary>
    public partial class EmailManagementView : UserControl
    {
        public EmailManagementView()
        {
            InitializeComponent();
            DataContextChanged += EmailManagementView_DataContextChanged;
        }

        private void EmailManagementView_DataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is EmailManagementViewModel viewModel)
            {
                // 加载数据
                viewModel.LoadDataCommand.Execute(null);
            }
        }
    }
}