using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Helper;
using DMS.WPF.Services;
using DMS.Services;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ViewModels.Items;
using iNKORE.UI.WPF.Modern.Common.IconKeys;

namespace DMS.WPF.ViewModels;

public partial class DeviceDetailViewModel : ViewModelBase, INavigatable
{
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly IVariableTableAppService _variableTableAppService;
    public DataServices DataServices { get; set; }

    [ObservableProperty]
    private DeviceItemViewModel _currentDevice;

    [ObservableProperty]
    private VariableItemViewModel _selectedVariableTable;

    public DeviceDetailViewModel(IMapper mapper, IDialogService dialogService, INavigationService navigationService,
                                 DataServices dataServices, IVariableTableAppService variableTableAppService)
    {
        _mapper = mapper;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _variableTableAppService = variableTableAppService;
        DataServices = dataServices;
    }

    [RelayCommand]
    private async Task AddVariableTable()
    {
        try
        {
            VariableTableDialogViewModel variableTableDialogViewModel = new VariableTableDialogViewModel()
                                                                        {
                                                                            PrimaryButContent = "添加变量表"
                                                                        };
            // 1. 显示添加设备对话框
            var variableTableItemViewModel = await _dialogService.ShowDialogAsync(variableTableDialogViewModel);
            // 如果用户取消或对话框未返回设备，则直接返回
            if (variableTableItemViewModel == null)
            {
                return;
            }

            CreateVariableTableWithMenuDto createDto = new CreateVariableTableWithMenuDto();
            createDto.VariableTable = _mapper.Map<VariableTableDto>(variableTableItemViewModel);
            createDto.DeviceId = CurrentDevice.Id;
            createDto.Menu = new MenuBeanDto()
                             {
                                 Header = variableTableItemViewModel.Name,
                                 Icon = SegoeFluentIcons.DataSense.Glyph
                             };
            CreateVariableTableWithMenuDto
                resCreateDto = await _variableTableAppService.CreateVariableTableAsync(createDto);
            DataServices.AddVariableTable(_mapper.Map<VariableTableItemViewModel>(resCreateDto.VariableTable));
            DataServices.AddMenuItem(_mapper.Map<MenuItemViewModel>(resCreateDto.Menu));
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
            // NotificationHelper.ShowInfo("请选择要编辑的变量表。");
            return;
        }
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


    public async Task OnNavigatedToAsync(MenuItemViewModel menu)
    {
        var device = DataServices.Devices.FirstOrDefault(d => d.Id == menu.TargetId);
        if (device != null)
        {
            CurrentDevice = device;
        }
    }

    [RelayCommand]
    public void NavigateToVariableTable()
    {
        if (SelectedVariableTable == null) return;
        var menu = DataServices.Menus.FirstOrDefault(m => m.MenuType == MenuType.VariableTableMenu &&
                                                          m.TargetId == SelectedVariableTable.Id);
        if (menu == null) return;
        _navigationService.NavigateToAsync(menu);
    }
}