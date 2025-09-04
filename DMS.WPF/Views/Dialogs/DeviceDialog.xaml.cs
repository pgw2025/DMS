using System.Windows.Controls;
using DMS.WPF.Helper;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.WPF.Views.Dialogs;

public partial class DeviceDialog : ContentDialog
{
    private const int ContentAreaMaxWidth = 1000;
    private const int ContentAreaMaxHeight = 800;

    public DeviceDialog()
    {
        InitializeComponent();
        this.Opened += OnOpened;

    }


    private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        //�޸ĶԻ������ݵ�����Ⱥ����߶�
        var backgroundElementBorder = VisualTreeFinder.FindVisualChildByName<Border>(this, "BackgroundElement");
        backgroundElementBorder.MaxWidth = ContentAreaMaxWidth;
        backgroundElementBorder.MaxWidth = ContentAreaMaxHeight;

    }

}