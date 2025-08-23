using DMS.WPF.Helper;
using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;
using System.Windows.Controls;

namespace DMS.WPF.Views.Dialogs;

public partial class VariableDialog 
{
    private const int ContentAreaMaxWidth = 1000;
    private const int ContentAreaMaxHeight = 800;
    public VariableDialog()
    {
        InitializeComponent();
        this.Opened += OnOpened;
    }

    private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        //修改对话框内容的最大宽度和最大高度
        var backgroundElementBorder = VisualTreeFinder.FindVisualChildByName<Border>(this, "BackgroundElement");
        backgroundElementBorder.MaxWidth = ContentAreaMaxWidth;
        backgroundElementBorder.MaxWidth = ContentAreaMaxHeight;

    }

}