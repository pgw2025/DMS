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
    private VariableTableItemViewModel _selectedVariableTable;

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
        try
        {
            if (SelectedVariableTable == null)
            {
                NotificationHelper.ShowError("你没有选择任何变量表，请选择变量表后再点击编辑变量表");
                return;
            }

            VariableTableDialogViewModel variableTableDialogViewModel = new VariableTableDialogViewModel(SelectedVariableTable)
                                                                          {
                                                                              PrimaryButContent = "编辑变量表"
                                                                          };
            // 1. 显示变量表对话框
            VariableTableItemViewModel variableTable = await _dialogService.ShowDialogAsync(variableTableDialogViewModel);
            // 如果用户取消或对话框未返回变量表，则直接返回
            if (variableTable == null)
            {
                return;
            }

            int res = await _variableTableAppService.UpdateVariableTableAsync(_mapper.Map<VariableTableDto>(variableTable));
            if (res > 0)
            {
                var menu = DataServices.Menus.FirstOrDefault(m =>
                                                                 m.MenuType == MenuType.VariableTableMenu &&
                                                                 m.TargetId == variableTable.Id);
                if (menu != null)
                {
                    menu.Header = variableTable.Name;
                }
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

            ConfrimDialogViewModel viewModel = new ConfrimDialogViewModel();
            viewModel.Message = $"确认要删除变量表名为:{SelectedVariableTable.Name} \n\n此操作将同时删除该变量表下的所有变量数据，且无法恢复！";
            viewModel.Title = "删除变量表";
            viewModel.PrimaryButContent = "删除";

            var resViewModel = await _dialogService.ShowDialogAsync(viewModel);
            if (resViewModel.IsPrimaryButton)
            {
                var isDel = await _variableTableAppService.DeleteVariableTableAsync(SelectedVariableTable.Id);
                if (isDel)
                {
                    var delName = SelectedVariableTable.Name;
                    // 更新界面
                    DataServices.DeleteVariableTableById(SelectedVariableTable.Id);

                    NotificationHelper.ShowSuccess($"删除变量表成功,变量表名：{delName}");
                }
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"删除变量表的过程中发生错误：{e.Message}", e);
        }
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