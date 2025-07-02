using System.Windows;
using System.Windows.Controls;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PMSWPF.Helper;
using PMSWPF.ViewModels;

namespace PMSWPF.Views;

public partial class VariableTableView : UserControl
{
    public bool IsLoadCompletion;

    public VariableTableView()
    {
        InitializeComponent();
        IsLoadCompletion = false;
    }


    private async void OnIsActiveChanged(object sender, RoutedEventArgs e)
    {
        try
        {
            var _viewModel = (VariableTableViewModel)this.DataContext;
            // 判断如果没有加载完成就跳过，防止ToggleSwtich加载的时候触发
            if (!_viewModel.IsLoadCompletion || !IsLoadCompletion)
                return;

            ToggleSwitch toggleSwitch = (ToggleSwitch)sender;
            await _viewModel.OnIsActiveChanged(toggleSwitch.IsOn);
        }
        catch (Exception exception)
        {
            NotificationHelper.ShowMessage($"修改变量表启用，停用时发生了错误：{exception.Message}");
        }
    }

    private void VariableTableView_OnLoaded(object sender, RoutedEventArgs e)
    {
        IsLoadCompletion = true;
    }
}