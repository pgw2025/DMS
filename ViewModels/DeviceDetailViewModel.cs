using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PMSWPF.Data.Repositories;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

public partial class DeviceDetailViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly VarTableRepository _varTableRepository;

    [ObservableProperty]
    private Device _currentDevice;

    [ObservableProperty]
    private VariableTable _selectedVariableTable;

    public DeviceDetailViewModel(IDialogService dialogService, VarTableRepository varTableRepository)
    {
        _dialogService = dialogService;
        _varTableRepository = varTableRepository;
    }

    public override void OnLoaded()
    {
        // Ensure VariableTables is an ObservableCollection for UI binding
        // if (_curentDevice.VariableTables != null &&
        //     !(_curentDevice.VariableTables is ObservableCollection<VariableTable>))
        // {
        //     _curentDevice.VariableTables = new ObservableCollection<VariableTable>(_curentDevice.VariableTables);
        // }
        // else if (_curentDevice.VariableTables == null)
        // {
        //     _curentDevice.VariableTables = new ObservableCollection<VariableTable>();
        // }
    }

    [RelayCommand]
    private async Task AddVariableTable()
    {
        try
        {
            var newVarTable = await _dialogService.ShowAddVarTableDialog();
            if (newVarTable == null) return;

            newVarTable.DeviceId = CurrentDevice.Id;
            newVarTable = await _varTableRepository.AddAsync(newVarTable);
            if (newVarTable != null)
            {
                CurrentDevice.VariableTables.Add(newVarTable);
                NotificationHelper.ShowSuccess($"变量表 {newVarTable.Name} 添加成功。");
            }
            else
            {
                NotificationHelper.ShowError($"变量表 {newVarTable.Name} 添加失败。");
            }
        }
        catch (Exception ex)
        {
            NotificationHelper.ShowError($"添加变量表时发生错误: {ex.Message}", ex);
        }
    }

    [RelayCommand]
    private async Task EditVariableTable()
    {
        if (SelectedVariableTable == null)
        {
            NotificationHelper.ShowInfo("请选择要编辑的变量表。");
            return;
        }

        try
        {
            var editedVarTable = await _dialogService.ShowEditVarTableDialog(SelectedVariableTable);
            if (editedVarTable == null) return;

            // The dialog already updated the SelectedVariableTable if it returned a non-null value
            // So we just need to save it to the database
            var result = await _varTableRepository.UpdateAsync(SelectedVariableTable);
            if (result > 0)
            {
                NotificationHelper.ShowSuccess($"变量表 {SelectedVariableTable.Name} 编辑成功。");
            }
            else
            {
                NotificationHelper.ShowError($"变量表 {SelectedVariableTable.Name} 编辑失败。");
            }
        }
        catch (Exception ex)
        {
            NotificationHelper.ShowError($"编辑变量表时发生错误: {ex.Message}", ex);
        }
    }

    [RelayCommand]
    private async Task DeleteVariableTable()
    {
        if (SelectedVariableTable == null)
        {
            NotificationHelper.ShowInfo("请选择要删除的变量表。");
            return;
        }

        var confirm = await _dialogService.ShowConfrimeDialog(
            "删除确认",
            $"确定要删除变量表 \"{SelectedVariableTable.Name}\" 吗?\n\n此操作将同时删除该变量表下的所有变量数据，且无法恢复！",
            "删除");

        if (!confirm) return;

        try
        {
            var result = await _varTableRepository.DeleteAsync(SelectedVariableTable);
            if (result > 0)
            {
                CurrentDevice.VariableTables.Remove(SelectedVariableTable);
                NotificationHelper.ShowSuccess($"变量表 {SelectedVariableTable.Name} 删除成功。");
            }
            else
            {
                NotificationHelper.ShowError($"变量表 {SelectedVariableTable.Name} 删除失败。");
            }
        }
        catch (Exception ex)
        {
            NotificationHelper.ShowError($"删除变量表时发生错误: {ex.Message}", ex);
        }
    }

    // Placeholder for EditDeviceCommand and DeleteDeviceCommand if they are needed here
    [RelayCommand]
    private async Task EditDevice()
    {
        // Implement device editing logic, similar to AddDeviceCommand but for existing device
        NotificationHelper.ShowInfo("编辑设备功能待实现。");
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DeleteDevice()
    {
        // Implement device deletion logic
        NotificationHelper.ShowInfo("删除设备功能待实现。");
        await Task.CompletedTask;
    }
}