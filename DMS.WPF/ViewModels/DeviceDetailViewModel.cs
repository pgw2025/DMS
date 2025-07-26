using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.WPF.Services;
using DMS.Services;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.ViewModels;

public partial class DeviceDetailViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly DataServices _dataServices;

    [ObservableProperty]
    private DeviceItemViewModel _currentDevice;

    [ObservableProperty]
    private VariableItemViewModel _selectedVariableTable;

    public DeviceDetailViewModel(IDialogService dialogService, DataServices dataServices)
    {
        _dialogService = dialogService;
        _dataServices = dataServices;
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
        // using var db = DbContext.GetInstance();
        // try
        // {
        //     // 1. Show dialog to get new variable table details
        //     var newVarTable = await _dialogService.ShowAddVarTableDialog();
        //     if (newVarTable == null) return; // User cancelled
        //
        //     // 2. Set properties for the new variable table
        //     newVarTable.DeviceId = CurrentDevice.Id;
        //     newVarTable.ProtocolType = CurrentDevice.ProtocolType;
        //     newVarTable.IsActive = true;
        //
        //     // 3. Find the parent menu for the current device
        //     var parentMenu = DataServicesHelper.FindMenusForDevice(CurrentDevice, _dataServices.MenuTrees);
        //     if (parentMenu == null)
        //     {
        //         //NotificationHelper.ShowError("无法找到当前设备的父级菜单，无法添加变量表菜单。");
        //         return;
        //     }
        //
        //     // 4. Start database transaction
        //     await db.BeginTranAsync();
        //
        //     // 5. AddAsync variable table to the database
        //     var addedVarTable = await _varTableRepository.AddAsync(newVarTable, db);
        //
        //     // 6. Create and add the corresponding menu item
        //     var newMenu = new MenuBean
        //                   {
        //                       Name = addedVarTable.Name,
        //                       DataId = addedVarTable.Id,
        //                       Type = MenuType.VariableTableMenu,
        //                       ParentId = parentMenu.Id,
        //                       Icon = iNKORE.UI.WPF.Modern.Common.IconKeys.SegoeFluentIcons.Tablet.Glyph
        //                   };
        //     await _menuRepository.AddAsync(newMenu, db);
        //
        //     // 7. Commit transaction
        //     await db.CommitTranAsync();
        //
        //     // 8. Update UI
        //     CurrentDevice?.VariableTables?.Add(addedVarTable);
        //     MessageHelper.SendLoadMessage(Enums.LoadTypes.Menu); // Refresh the main navigation menu
        //     //NotificationHelper.ShowSuccess($"变量表 {addedVarTable.Name} 添加成功。");
        // }
        // catch (Exception ex)
        // {
        //     await db.RollbackTranAsync();
        //     //NotificationHelper.ShowError($"添加变量表时发生错误: {ex.Message}", ex);
        // }
    }

    [RelayCommand]
    private async Task EditVariableTable()
    {
        // if (SelectedVariableTable == null)
        // {
        //     //NotificationHelper.ShowInfo("请选择要编辑的变量表。");
        //     return;
        // }
        //
        // using var db = DbContext.GetInstance();
        // try
        // {
        //     var originalName = SelectedVariableTable.Name; // Store original name for comparison
        //     var editedVarTable = await _dialogService.ShowEditVarTableDialog(SelectedVariableTable);
        //     if (editedVarTable == null) return;
        //
        //     await db.BeginTranAsync();
        //
        //     // Update variable table in DB
        //     var result = await _varTableRepository.UpdateAsync(SelectedVariableTable, db);
        //
        //     if (result > 0)
        //     {
        //         // Update corresponding menu item if name changed
        //         if (originalName != SelectedVariableTable.Name)
        //         {
        //             var menu = DataServicesHelper.FindVarTableMenu(SelectedVariableTable.Id, _dataServices.MenuTrees);
        //             if (menu != null)
        //             {
        //                 menu.Name = SelectedVariableTable.Name;
        //                 await _menuRepository.UpdateAsync(menu, db);
        //             }
        //         }
        //
        //         await db.CommitTranAsync();
        //         //NotificationHelper.ShowSuccess($"变量表 {SelectedVariableTable.Name} 编辑成功。");
        //         MessageHelper.SendLoadMessage(Enums.LoadTypes.Menu); // Refresh the main navigation menu
        //     }
        //     else
        //     {
        //         await db.RollbackTranAsync();
        //         //NotificationHelper.ShowError($"变量表 {SelectedVariableTable.Name} 编辑失败。");
        //     }
        // }
        // catch (Exception ex)
        // {
        //     await db.RollbackTranAsync();
        //     //NotificationHelper.ShowError($"编辑变量表时发生错误: {ex.Message}", ex);
        // }
    }

    [RelayCommand]
    private async Task DeleteVariableTable()
    {
        // if (SelectedVariableTable == null)
        // {
        //     //NotificationHelper.ShowInfo("请选择要删除的变量表。");
        //     return;
        // }
        //
        // var confirm = await _dialogService.ShowConfrimeDialog(
        //     "删除确认",
        //     $"确定要删除变量表 \"{SelectedVariableTable.Name}\" 吗?\n\n此操作将同时删除该变量表下的所有变量数据，且无法恢复！",
        //     "删除");
        //
        // if (!confirm) return;
        //
        // using var db = DbContext.GetInstance();
        // try
        // {
        //     await db.BeginTranAsync();
        //
        //     // Find the corresponding menu item
        //     MenuBean menuToDelete = null;
        //     if (_dataServices.MenuTrees != null)
        //     {
        //         menuToDelete = DataServicesHelper.FindVarTableMenu( SelectedVariableTable.Id,_dataServices.MenuTrees);
        //     }
        //
        //     // Delete variable table from DB
        //     var result = await _varTableRepository.DeleteAsync(SelectedVariableTable, db);
        //
        //     if (result > 0)
        //     {
        //         // Delete corresponding menu item
        //         if (menuToDelete != null)
        //         {
        //             await _menuRepository.DeleteAsync(menuToDelete, db);
        //         }
        //
        //         await db.CommitTranAsync();
        //         var delVarTableName = SelectedVariableTable.Name;
        //         CurrentDevice?.VariableTables?.Remove(SelectedVariableTable);
        //         //NotificationHelper.ShowSuccess($"变量表 {delVarTableName} 删除成功。");
        //         MessageHelper.SendLoadMessage(Enums.LoadTypes.Menu); // Refresh the main navigation menu
        //     }
        //     else
        //     {
        //         await db.RollbackTranAsync();
        //         //NotificationHelper.ShowError($"变量表 {SelectedVariableTable.Name} 删除失败。");
        //     }
        // }
        // catch (Exception ex)
        // {
        //     await db.RollbackTranAsync();
        //     //NotificationHelper.ShowError($"删除变量表时发生错误: {ex.Message}", ex);
        // }
    }

    // Placeholder for EditDeviceCommand and DeleteDeviceCommand if they are needed here
    [RelayCommand]
    private async Task EditDevice()
    {
        // Implement device editing logic, similar to AddDeviceCommand but for existing device
        //NotificationHelper.ShowInfo("编辑设备功能待实现。");
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task DeleteDevice()
    {
        // Implement device deletion logic
        //NotificationHelper.ShowInfo("删除设备功能待实现。");
        await Task.CompletedTask;
    }

    [RelayCommand]
    private void NavigateToVariableTable()
    {
        // if (SelectedVariableTable == null) return;
        //
        // var variableTableVm = App.Current.Services.GetRequiredService<VariableTableViewModel>();
        // variableTableVm.VariableTable = SelectedVariableTable;
        // MessageHelper.SendNavgatorMessage(variableTableVm);
    }
}