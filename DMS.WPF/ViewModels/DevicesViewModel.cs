using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Helper;
using DMS.Services;
using DMS.WPF.Helper;
using DMS.WPF.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ViewModels.Items;
using iNKORE.UI.WPF.Modern.Common.IconKeys;

namespace DMS.WPF.ViewModels;

/// <summary>
/// 设备管理视图模型，负责设备的增删改查操作。
/// </summary>
public partial class DevicesViewModel : ViewModelBase, INavigatable
{
    public DataServices DataServices { get; }
    private readonly IDeviceAppService _deviceAppService;
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;


    /// <summary>
    /// 设备列表。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<DeviceItemViewModel> _devices;


    /// <summary>
    /// 当前选中的设备。
    /// </summary>
    [ObservableProperty]
    private DeviceItemViewModel _selectedDevice;

    /// <summary>
    /// 初始化 <see cref="DevicesViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="logger">日志记录器。</param>
    /// <param name="dialogService">对话框服务。</param>
    /// <param name="dataServices">数据服务。</param>
    public DevicesViewModel(IMapper mapper,
                            IDialogService dialogService, INavigationService navigationService,
                            DataServices dataServices, IDeviceAppService deviceAppService)
    {
        _mapper = mapper;
        _dialogService = dialogService;
        _navigationService = navigationService;
        DataServices = dataServices;
        _deviceAppService = deviceAppService;
        Devices = new ObservableCollection<DeviceItemViewModel>();
        DataServices.OnDeviceListChanged += (devices) => { };
    }


    /// <summary>
    /// 添加设备命令。
    /// </summary>
    [RelayCommand]
    public async Task AddDevice()
    {
        try
        {
            // 1. 显示添加设备对话框
            DeviceItemViewModel device = await _dialogService.ShowDialogAsync(new DeviceDialogViewModel()
                                                                              {
                                                                                  Title = "添加设备",
                                                                                  PrimaryButText = "添加设备"
                                                                              });
            // 如果用户取消或对话框未返回设备，则直接返回
            if (device == null)
            {
                return;
            }


            CreateDeviceWithDetailsDto dto = new CreateDeviceWithDetailsDto();
            dto.Device = _mapper.Map<DeviceDto>(device);


            dto.DeviceMenu = new MenuBeanDto()
                             {
                                 Header = device.Name,
                                 Icon = SegoeFluentIcons.Devices2.Glyph,
                                 TargetViewKey = "DeviceDetailView"
                             };
            if (device.IsAddDefVarTable)
            {
                dto.VariableTable = new VariableTableDto()
                                    {
                                        Name = "默认变量表",
                                        Description = "默认变量表",
                                        IsActive = true
                                    };
                dto.VariableTableMenu = new MenuBeanDto()
                                        {
                                            Header = dto.VariableTable.Name,
                                            Icon = SegoeFluentIcons.DataSense.Glyph,
                                            TargetViewKey = "VariableTableView"
                                        };
            }


            // 添加设备
            var addDto = await DataServices.AddDevice(dto);

            NotificationHelper.ShowSuccess($"设备添加成功：{addDto.Device.Name}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            NotificationHelper.ShowError($"添加设备的过程中发生错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 删除设备命令。
    /// </summary>
    [RelayCommand]
    private async Task DeleteDevice()
    {
        try
        {
            if (SelectedDevice == null)
            {
                NotificationHelper.ShowError("你没有选择任何设备，请选择设备后再点击删除设备");
                return;
            }

            var viewModel = new ConfirmDialogViewModel("删除设备", $"确认要删除设备名为:{SelectedDevice.Name}", "删除设备");
            if (await _dialogService.ShowDialogAsync(viewModel))
            {
                var deviceName = SelectedDevice.Name;
                if (await DataServices.DeleteDevice(SelectedDevice))
                {
                    NotificationHelper.ShowSuccess($"删除设备成功,设备名：{deviceName}");
                }
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"删除设备的过程中发生错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 编辑设备命令。
    /// </summary>
    [RelayCommand]
    private async Task EditDevice()
    {
        try
        {
            if (SelectedDevice == null)
            {
                NotificationHelper.ShowError("你没有选择任何设备，请选择设备后再点击编辑设备");
                return;
            }

            DeviceDialogViewModel deviceDialogViewModel = new DeviceDialogViewModel(SelectedDevice)
                                                          {
                                                              PrimaryButText = "编辑设备"
                                                          };
            // 1. 显示设备对话框
            DeviceItemViewModel device = await _dialogService.ShowDialogAsync(deviceDialogViewModel);
            // 如果用户取消或对话框未返回设备，则直接返回
            if (device == null)
            {
                return;
            }

            if (await DataServices.UpdateDevice(device))
            {
                NotificationHelper.ShowSuccess($"编辑设备成功：{device.Name}");
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"编辑设备的过程中发生错误：{e.Message}", e);
        }
    }

    [RelayCommand]
    public void NavigateToDetail()
    {
        if (SelectedDevice == null) return;

        var menu = DataServices.Menus.FirstOrDefault(m => m.MenuType == MenuType.DeviceMenu &&
                                                          m.TargetId == SelectedDevice.Id);
        if (menu == null) return;

        _navigationService.NavigateToAsync(menu);
    }

    public async Task OnNavigatedToAsync(MenuItemViewModel menu)
    {
    }
}