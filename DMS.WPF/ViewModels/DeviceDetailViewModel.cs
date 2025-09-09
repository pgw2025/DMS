using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dm;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.WPF.Services;
using DMS.Services;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ViewModels.Items;
using iNKORE.UI.WPF.Modern.Common.IconKeys;

namespace DMS.WPF.ViewModels;

public partial class DeviceDetailViewModel : ViewModelBase, INavigatable
{
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly IDataStorageService _dataStorageService;
    private readonly INavigationService _navigationService;
    private readonly IWPFDataService _wpfDataService;

    [ObservableProperty]
    private DeviceItemViewModel _currentDevice;

    [ObservableProperty]
    private VariableTableItemViewModel _selectedVariableTable;

    private readonly INotificationService _notificationService;
    
    

    public DeviceDetailViewModel(IMapper mapper, IDialogService dialogService,IDataStorageService dataStorageService ,INavigationService navigationService,
                                 IWPFDataService wpfDataService, INotificationService notificationService)
    {
        _mapper = mapper;
        _dialogService = dialogService;
        _dataStorageService = dataStorageService;
        _navigationService = navigationService;
        _notificationService = notificationService;
        _wpfDataService = wpfDataService;
    }

    [RelayCommand]
    private async Task AddVariableTable()
    {
        try
        {
            VariableTableDialogViewModel variableTableDialogViewModel = new VariableTableDialogViewModel()
                                                                        {
                                                                            PrimaryButText = "添加变量表"
                                                                        };
            // 1. 显示添加设备对话框
            var variableTableItemViewModel = await _dialogService.ShowDialogAsync(variableTableDialogViewModel);
            // 如果用户取消或对话框未返回设备，则直接返回
            if (variableTableItemViewModel == null)
            {
                return;
            }

            variableTableItemViewModel.DeviceId = CurrentDevice.Id;
             var tableMenu = new MenuBeanDto()
                             {
                                 Header = variableTableItemViewModel.Name,
                                 Icon = SegoeFluentIcons.DataSense.Glyph,
                                 TargetViewKey = "VariableTableView"
                             };

             if (await _wpfDataService.VariableTableDataService.AddVariableTable(_mapper.Map<VariableTableDto>(variableTableItemViewModel),
                                                    tableMenu, true))
             {
                 _notificationService.ShowSuccess($"添加变量表成功：{variableTableItemViewModel.Name}");
             }
             else
             {
                 _notificationService.ShowError($"添加变量表失败：{variableTableItemViewModel.Name}！！");
             }

        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"添加变量表时发生错误: {ex.Message}", ex);
        }
    }

    [RelayCommand]
    private async Task EditVariableTable()
    {
        try
        {
            if (SelectedVariableTable == null)
            {
                _notificationService.ShowError("你没有选择任何变量表，请选择变量表后再点击编辑变量表");
                return;
            }

            VariableTableDialogViewModel variableTableDialogViewModel = new VariableTableDialogViewModel(SelectedVariableTable)
                                                                          {
                                                                              PrimaryButText = "编辑变量表"
                                                                          };
            // 1. 显示变量表对话框
            VariableTableItemViewModel variableTable = await _dialogService.ShowDialogAsync(variableTableDialogViewModel);
            // 如果用户取消或对话框未返回变量表，则直接返回
            if (variableTable == null)
            {
                return;
            }

            if (await _wpfDataService.VariableDataService.UpdateVariableTable(variableTable))
            {
                _notificationService.ShowSuccess($"编辑变量表成功：{variableTable.Name}");
            }
            else
            {
                _notificationService.ShowError($"编辑变量表失败：{variableTable.Name}");
            }
        }
        catch (Exception e)
        {
            _notificationService.ShowError($"编辑变量表的过程中发生错误：{e.Message}", e);
        }
    }

    [RelayCommand]
    private async Task DeleteVariableTable()
    {
        try
        { 
            if (SelectedVariableTable == null)
            {
                _notificationService.ShowError("你没有选择任何变量表，请选择变量表后再点击删除变量表");
                return;
            }

            string message = $"确认要删除变量表名为:{SelectedVariableTable.Name} \n\n此操作将同时删除该变量表下的所有变量数据，且无法恢复！";
            ConfirmDialogViewModel viewModel = new ConfirmDialogViewModel("删除变量表",message,"删除");
            if (await _dialogService.ShowDialogAsync(viewModel))
            {
                var tableName = SelectedVariableTable.Name;
                if (await _wpfDataService.VariableDataService.DeleteVariableTable(SelectedVariableTable,true))
                {
                    _notificationService.ShowSuccess($"变量表：{tableName},删除成功。");
                }
                else
                {
                    _notificationService.ShowError($"变量表：{tableName},删除失败!!!");
                }
            }
        }
        catch (Exception e)
        {
            _notificationService.ShowError($"删除变量表的过程中发生错误：{e.Message}", e);
        }
    }



    public async Task OnNavigatedToAsync(MenuItemViewModel menu)
    {
        var device = _dataStorageService.Devices.FirstOrDefault(d => d.Id == menu.TargetId);
        if (device != null)
        {
            CurrentDevice = device;
        }
    }

    [RelayCommand]
    public void NavigateToVariableTable()
    {
        if (SelectedVariableTable == null) return;
        var menu = _dataStorageService.Menus.FirstOrDefault(m => m.MenuType == MenuType.VariableTableMenu &&
                                                                 m.TargetId == SelectedVariableTable.Id);
        if (menu == null) return;
        _navigationService.NavigateToAsync(menu);
    }
}