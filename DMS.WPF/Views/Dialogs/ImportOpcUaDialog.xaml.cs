using DMS.Helper;
using DMS.Services;
using DMS.WPF.Helper;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ViewModels.Items;
using iNKORE.UI.WPF.Modern.Controls;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DMS.WPF.Views.Dialogs;

/// <summary>
/// ImportOpcUaDialog.xaml 的交互逻辑
/// </summary>
public partial class ImportOpcUaDialog : ContentDialog
{
    private const int ContentAreaMaxWidth = 1300;
    private const int ContentAreaMaxHeight = 900;




    public ImportOpcUaDialog()
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

    private async void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        try
        {
            //防止多次调用
            object selectedObj = this.treeView.SelectedItem;
            if (SelectedItemChanged != null)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Background, SelectedItemChanged, selectedObj);
            }
        }
        catch (Exception ex)
        {
            NotificationHelper.ShowError($"选择节点时发生了错误：{ex.Message}");
        }


    }

    //事件
    public async void SelectedItemChanged(object selectedObj)
    {
        if (selectedObj is OpcUaNodeItemViewModel selectedNode)
        {

            if (this.DataContext is ImportOpcUaDialogViewModel viewModel)
            {
                await viewModel.LoadNodeVariables(selectedNode);
            }
        }
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
