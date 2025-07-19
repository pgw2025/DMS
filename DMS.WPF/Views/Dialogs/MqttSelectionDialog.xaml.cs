using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;
using DMS.WPF.Models;

namespace DMS.Views.Dialogs;

public partial class MqttSelectionDialog : ContentDialog
{
    public MqttSelectionDialog(MqttSelectionDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // 在这里可以添加一些验证逻辑，例如确保选择了MQTT服务器
        var viewModel = (MqttSelectionDialogViewModel)DataContext;
        if (viewModel.SelectedMqtt == null)
        {
            args.Cancel = true; // 取消关闭对话框
            // 可以显示一个警告消息
        }
    }

    private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // 用户点击取消，不需要额外处理
    }
}