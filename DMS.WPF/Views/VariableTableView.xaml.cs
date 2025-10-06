using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DMS.WPF.ViewModels;
using DMS.WPF.Interfaces;
using DMS.WPF.Services;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using DMS.Core.Enums;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.Views;

public partial class VariableTableView : UserControl
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public bool IsLoadCompletion;
    private VariableTableViewModel _viewModel;

    public VariableTableView()
    {
        InitializeComponent();
        IsLoadCompletion = false;
    }


    private async void OnIsActiveChanged(object sender, RoutedEventArgs e)
    {
        try
        {
            _viewModel = (VariableTableViewModel)this.DataContext;
            // 判断如果没有加载完成就跳过，防止ToggleSwtich加载的时候触发
            if (!_viewModel.IsLoadCompletion || !IsLoadCompletion)
                return;

            ToggleSwitch toggleSwitch = (ToggleSwitch)sender;
            await _viewModel.OnIsActiveChanged(toggleSwitch.IsOn);
        }
        catch (Exception exception)
        {
            var notificationService = App.Current.Services.GetRequiredService<INotificationService>();
            notificationService.ShowError($"修改变量表启用，停用时发生了错误：{exception.Message}", exception);
        }
    }

    private void VariableTableView_OnLoaded(object sender, RoutedEventArgs e)
    {
        IsLoadCompletion = true;
    }
    
   
}