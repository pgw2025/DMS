using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.Models;
using PMSWPF.ViewModels.Dialogs;

namespace PMSWPF.Views.Dialogs;

/// <summary>
/// OpcUaImportDialog.xaml 的交互逻辑
/// </summary>
public partial class OpcUaImportDialog : ContentDialog
{
    public OpcUaImportDialogViewModel ViewModel
    {
        get => (OpcUaImportDialogViewModel)DataContext;
        set => DataContext = value;
    }

    public OpcUaImportDialog(OpcUaImportDialogViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // 在这里处理导入逻辑，例如获取选中的变量
        // ViewModel.ImportSelectedVariables();
    }

    private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // 处理取消逻辑
    }

    private async void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is OpcUaNode selectedNode)
        {
            await ViewModel.LoadNodeVariables(selectedNode);
        }
    }
}
