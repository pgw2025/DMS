using System.Windows;
using System.Windows.Controls;
using DMS.WPF.ViewModels.Dialogs;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.WPF.Views.Dialogs;

/// <summary>
/// ImportOpcUaDialog.xaml 的交互逻辑
/// </summary>
public partial class ImportOpcUaDialog : ContentDialog
{
    public ImportOpcUaDialogViewModel ViewModel
    {
        get => (ImportOpcUaDialogViewModel)DataContext;
        set => DataContext = value;
    }

    public ImportOpcUaDialog(ImportOpcUaDialogViewModel viewModel)
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
        // if (e.NewValue is OpcUaNode selectedNode)
        // {
        //     await ViewModel.LoadNodeVariables(selectedNode);
        // }
    }


    private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs args)
    {
       // if (args.AddedItems!=null && args.AddedItems.Count>0)
       //  {
       //      foreach (var item in args.AddedItems)
       //      {
       //          ViewModel.SelectedVariables.Add((Variable)item);
       //      }
       //      
       //  } 
    }
}
