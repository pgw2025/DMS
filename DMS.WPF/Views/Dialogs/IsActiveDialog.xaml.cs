using System.Windows;
using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.Views.Dialogs;

public partial class IsActiveDialog : ContentDialog
{
    public IsActiveDialogViewModel ViewModel { get; }

    public IsActiveDialog(IsActiveDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // 确认按钮点击时，ViewModel.SelectedIsActive 已经通过绑定更新
        // 如果用户没有选择任何选项，可以阻止关闭对话框
        if (!ViewModel.SelectedIsActive.HasValue)
        {
            args.Cancel = true;
        }
    }

    private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // 取消按钮点击时，将 SelectedIsActive 设置为 null
        ViewModel.SelectedIsActive = null;
    }
}