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

}