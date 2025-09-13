using DMS.WPF.Helper;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows;
using System.Windows.Controls;

namespace DMS.WPF.Views.Dialogs
{
    /// <summary>
    /// EmailTemplateDialog.xaml 的交互逻辑
    /// </summary>
    public partial class EmailTemplateDialog : ContentDialog
    {
        private const int ContentAreaMaxWidth = 1000;
        private const int ContentAreaMaxHeight = 800;

        public EmailTemplateDialog()
        {
            InitializeComponent();
            this.Opened += OnOpened;
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
    }
}