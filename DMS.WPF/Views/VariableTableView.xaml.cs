using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using DMS.Helper;
using DMS.WPF.Models;
using DMS.WPF.ViewModels;
using iNKORE.UI.WPF.Modern.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using DMS.Core.Enums;

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
            NotificationHelper.ShowError($"修改变量表启用，停用时发生了错误：{exception.Message}", exception);
        }
    }

    private void VariableTableView_OnLoaded(object sender, RoutedEventArgs e)
    {
        IsLoadCompletion = true;
    }

    private void DataGrid_OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs args)
    {
        if (args.EditAction != DataGridEditAction.Commit)
            return;

        try
        {
            // 获取到改变后的值和绑定的属性名
            Variable var = (Variable)args.Row.Item;
            var element = args.EditingElement;
            object newValue = null;
            string bindingPath = "";

            if (element is TextBox textBox)
            {
                newValue = textBox.Text;
                DataGridTextColumn textColumn = (DataGridTextColumn)args.Column;
                bindingPath = (textColumn.Binding as Binding)?.Path.Path;
            }
            else if (element is CheckBox checkBox)
            {
                newValue = checkBox.IsChecked;
                DataGridCheckBoxColumn checkBoxColumn = (DataGridCheckBoxColumn)args.Column;
                bindingPath = (checkBoxColumn.Binding as Binding)?.Path.Path;
            }
            else if (args.Column.Header.ToString() == "信号类型")
            {
                var comboBox = VisualTreeHelper.GetChild(element, 0) as ComboBox;
                if (comboBox != null)
                {
                    newValue = comboBox.SelectedItem;
                    bindingPath = "SignalType";
                }
            }
            else
            {
                return;
            }

            if (newValue == null || string.IsNullOrEmpty(bindingPath))
                return;
            // 通过反射拿到值
            var pathPropertyInfo = var.GetType()
                                          .GetProperty(bindingPath);
            var oldValue = pathPropertyInfo.GetValue(var);
            // 判断值是否相等
            if (newValue.ToString() != oldValue?.ToString())
            {
                var.IsModified = true;
            }

        }
        catch (Exception e)
        {
            NotificationHelper.ShowError("变量表编辑的过过程中发生了错误：" + e.Message, e);
        }
    }

    private async void DeleteVarData_Click(object sender, RoutedEventArgs e)
    {
        _viewModel = (VariableTableViewModel)this.DataContext;
        var selectedVariables = BasicGridView.SelectedItems.Cast<Variable>().ToList();
        if (selectedVariables.Any())
        {
            await _viewModel.DeleteVarData(selectedVariables);
        }
        else
        {
            NotificationHelper.ShowInfo("请选择要删除的变量");
        }
    }
}