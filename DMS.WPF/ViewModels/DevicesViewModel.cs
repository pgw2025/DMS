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
                            IDialogService dialogService, DataServices dataServices, IDeviceAppService deviceAppService)
    {
        _mapper = mapper;
        _dialogService = dialogService;
        DataServices = dataServices;
        _deviceAppService = deviceAppService;
        Devices = new ObservableCollection<DeviceItemViewModel>();
        DataServices.OnDeviceListChanged += (devices) => { };
    }


    public override Task<bool> OnExitAsync()
    {
        // if (_dataServices.Devices!=null && _dataServices.Devices.Count>0)
        // {
        //     foreach (var device in Devices)
        //     {
        //         device.OnDeviceIsActiveChanged -= HandleDeviceIsActiveChanged;
        //     }
        // }

        return Task.FromResult(true);
    }

    /// <summary>
    /// 添加设备命令。
    /// </summary>
    [RelayCommand]
    public async Task AddDevice()
    {
        try
        {
            DeviceDialogViewModel deviceDialogViewModel = new DeviceDialogViewModel()
                                                          {
                                                              PrimaryButContent = "添加设备"
                                                          };
            // 1. 显示添加设备对话框
            DeviceItemViewModel device = await _dialogService.ShowDialogAsync(deviceDialogViewModel);
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
                                 TargetViewKey = "DevicesView"
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

            var addDto = await _deviceAppService.CreateDeviceWithDetailsAsync(dto);

            // 更新界面
            DataServices.Devices.Add(_mapper.Map<DeviceItemViewModel>(addDto.Device));
            DataServices.AddMenuItem(_mapper.Map<MenuBeanItemViewModel>(addDto.DeviceMenu));
            DataServices.AddVariableTable(_mapper.Map<VariableTableItemViewModel>(addDto.VariableTable));
            DataServices.AddMenuItem(_mapper.Map<MenuBeanItemViewModel>(addDto.VariableTableMenu));

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
    public async void DeleteDevice()
    {
        try
        {
            if (SelectedDevice == null)
            {
                NotificationHelper.ShowError("你没有选择任何设备，请选择设备后再点击删除设备");
                return;
            }

            ConfrimDialogViewModel viewModel = new ConfrimDialogViewModel();
            viewModel.Message = $"确认要删除设备名为:{SelectedDevice.Name}";
            viewModel.Title = "删除设备";
            viewModel.PrimaryButContent = "删除";

            var resViewModel = await _dialogService.ShowDialogAsync(viewModel);
            if (resViewModel.IsPrimaryButton)
            {
                var isDel = await _deviceAppService.DeleteDeviceByIdAsync(SelectedDevice.Id);
                if (isDel)
                {
                    // 更新界面
                    DataServices.DeleteDeviceById(SelectedDevice.Id);

                    NotificationHelper.ShowSuccess($"删除设备成功,设备名：{SelectedDevice.Name}");
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
    public async void EditDevice()
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
                                                              PrimaryButContent = "编辑设备"
                                                          };
            // 1. 显示设备对话框
            DeviceItemViewModel device = await _dialogService.ShowDialogAsync(deviceDialogViewModel);
            // 如果用户取消或对话框未返回设备，则直接返回
            if (device == null)
            {
                return;
            }

            int res = await _deviceAppService.UpdateDeviceAsync(_mapper.Map<DeviceDto>(device));
            if (res > 0)
            {
                var menu = DataServices.Menus.FirstOrDefault(m =>
                                                                 m.MenuType == MenuType.DeviceMenu &&
                                                                 m.TargetId == device.Id);
                if (menu!=null)
                {
                    menu.Header=device.Name;
                }
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
        var deviceDetailVm = App.Current.Services.GetRequiredService<DeviceDetailViewModel>();
        deviceDetailVm.CurrentDevice = SelectedDevice;
        MessageHelper.SendNavgatorMessage(deviceDetailVm);
    }

    public async Task OnNavigatedToAsync(object parameter)
    {
    }
}