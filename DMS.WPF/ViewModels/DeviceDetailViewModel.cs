using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dm;
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
    public DataServices DataServices { get;  }

    [ObservableProperty]
    private DeviceItemViewModel _currentDevice;

    [ObservableProperty]
    private VariableTableItemViewModel _selectedVariableTable;

    public DeviceDetailViewModel(IMapper mapper, IDialogService dialogService, INavigationService navigationService,
                                 DataServices dataServices)
    {
        _mapper = mapper;
        _dialogService = dialogService;
        _navigationService = navigationService;
        DataServices = dataServices;
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
                                 Icon = SegoeFluentIcons.DataSense.Glyph
                             };

             if (await DataServices.AddVariableTable(_mapper.Map<VariableTableDto>(variableTableItemViewModel),
                                                     tableMenu, true))
             {
                 NotificationHelper.ShowSuccess($"添加变量表成功：{variableTableItemViewModel.Name}");
             }
             else
             {
                 NotificationHelper.ShowError($"添加变量表失败：{variableTableItemViewModel.Name}！！");
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
        try
        {
            if (SelectedVariableTable == null)
            {
                NotificationHelper.ShowError("你没有选择任何变量表，请选择变量表后再点击编辑变量表");
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

            if (await DataServices.UpdateVariableTable(variableTable))
            {
                NotificationHelper.ShowSuccess($"编辑变量表成功：{variableTable.Name}");
            }
            else
            {
                NotificationHelper.ShowError($"编辑变量表失败：{variableTable.Name}");
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"编辑变量表的过程中发生错误：{e.Message}", e);
        }
    }

    [RelayCommand]
    private async Task DeleteVariableTable()
    {
        try
        { 
            if (SelectedVariableTable == null)
            {
                NotificationHelper.ShowError("你没有选择任何变量表，请选择变量表后再点击删除变量表");
                return;
            }

            string message = $"确认要删除变量表名为:{SelectedVariableTable.Name} \n\n此操作将同时删除该变量表下的所有变量数据，且无法恢复！";
            ConfirmDialogViewModel viewModel = new ConfirmDialogViewModel("删除变量表",message,"删除");
            if (await _dialogService.ShowDialogAsync(viewModel))
            {
                var tableName = SelectedVariableTable.Name;
                if (await DataServices.DeleteVariableTable(SelectedVariableTable,true))
                {
                    NotificationHelper.ShowSuccess($"变量表：{tableName},删除成功。");
                }
                else
                {
                    NotificationHelper.ShowError($"变量表：{tableName},删除失败!!!");
                }
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"删除变量表的过程中发生错误：{e.Message}", e);
        }
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