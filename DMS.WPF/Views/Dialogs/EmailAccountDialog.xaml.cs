using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.Helper;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using System.Windows.Controls;

namespace DMS.WPF.Views.Dialogs
{
    /// <summary>
    /// EmailAccountDialog.xaml 的交互逻辑
    /// </summary>
    public partial class EmailAccountDialog : ContentDialog
    {
        private const int ContentAreaMaxWidth = 1000;
        private const int ContentAreaMaxHeight = 800;

        public EmailAccountDialog()
        {
            InitializeComponent();
            this.Opened += OnOpened;
            DataContextChanged += EmailAccountDialog_DataContextChanged;
        }

        private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            // 修改对话框内容区域的最大宽度和高度
            var backgroundElementBorder = VisualTreeFinder.FindVisualChildByName<Border>(this, "BackgroundElement");
            if (backgroundElementBorder != null)
            {
                backgroundElementBorder.MaxWidth = ContentAreaMaxWidth;
                backgroundElementBorder.MaxHeight = ContentAreaMaxHeight;
            }
        }

        private void EmailAccountDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is EmailAccountDialogViewModel viewModel)
            {
                // 处理密码框
                PasswordBox.Password = viewModel.Password;
                PasswordBox.PasswordChanged += (s, args) =>
                {
                    viewModel.Password = PasswordBox.Password;
                };
            }
        }
    }
}