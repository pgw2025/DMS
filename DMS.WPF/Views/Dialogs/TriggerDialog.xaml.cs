using System.Windows.Controls;
using DMS.WPF.Helper;
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
    }
}