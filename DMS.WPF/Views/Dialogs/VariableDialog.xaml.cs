using DMS.WPF.Helper;
using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows.Controls;

namespace DMS.WPF.Views.Dialogs;

public partial class VariableDialog 
{
    private const int ContentAreaMaxWidth = 1200;
    private const int ContentAreaMaxHeight = 900;
    
    public VariableDialog()
    {
        InitializeComponent();
        this.Opened += OnOpened;
    }

    private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        var backgroundElementBorder = VisualTreeFinder.FindVisualChildByName<Border>(this, "BackgroundElement");
        backgroundElementBorder.MaxWidth = ContentAreaMaxWidth;
        backgroundElementBorder.MaxWidth = ContentAreaMaxHeight;
    }
    
    private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // 获取ViewModel
        if (DataContext is VariableDialogViewModel viewModel)
        {
            // 调用ViewModel的PrimaryButtonAsync方法
            bool isValid = await viewModel.PrimaryButtonAsync();
            
            // 如果验证失败，取消对话框关闭
            if (!isValid)
            {
                args.Cancel = true;
            }
        }
    }
}