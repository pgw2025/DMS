using System.Windows.Controls;
using System.Windows.Input;
using DMS.WPF.Helper;
using DMS.WPF.ViewModels.Items;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.WPF.Views.Dialogs
{
    /// <summary>
    /// TriggerDialog.xaml 的交互逻辑
    /// </summary>
    public partial class TriggerDialog : ContentDialog
    {
        private const int ContentAreaMaxWidth = 1000;
        private const int ContentAreaMaxHeight = 800;

        public TriggerDialog()
        {
            InitializeComponent();
            this.Opened += OnOpened;
        }

        private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            // 修改对话框内容的最大宽度和最大高度
            var backgroundElementBorder = VisualTreeFinder.FindVisualChildByName<Border>(this, "BackgroundElement");
            if (backgroundElementBorder != null)
            {
                backgroundElementBorder.MaxWidth = ContentAreaMaxWidth;
                backgroundElementBorder.MaxHeight = ContentAreaMaxHeight;
            }
        }
        
        /// <summary>
        /// 处理变量列表双击事件，将选中的变量添加到已选择列表
        /// </summary>
        private void VariableListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (VariableListBox.SelectedItem is VariableItemViewModel selectedVariable)
            {
                var viewModel = DataContext as ViewModels.Dialogs.TriggerDialogViewModel;
                viewModel?.AddVariable(selectedVariable);
            }
        }
        
        /// <summary>
        /// 处理移除变量按钮点击事件
        /// </summary>
        private void RemoveVariableButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is VariableItemViewModel variable)
            {
                var viewModel = DataContext as ViewModels.Dialogs.TriggerDialogViewModel;
                viewModel?.RemoveVariable(variable);
            }
        }
    }
}