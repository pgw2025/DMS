using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Core.Enums;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ItemViewModel;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using ObservableCollections;

namespace DMS.WPF.ViewModels;

/// <summary>
/// 设备管理视图模型，负责设备的增删改查操作。
/// </summary>
public partial class DevicesViewModel : ViewModelBase, INavigatable
{
    private readonly IWPFDataService _wpfDataService;
    private readonly IDeviceAppService _deviceAppService;
    private readonly IMapper _mapper;
    private readonly IDataStorageService _dataStorageService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;


    /// <summary>
    /// 设备列表。
    /// </summary>
    [ObservableProperty]
    private INotifyCollectionChangedSynchronizedViewList<DeviceItem> _devices;


    /// <summary>
    /// 当前选中的设备。
    /// </summary>
    [ObservableProperty]
    private DeviceItem _selectedDevice;

    private readonly INotificationService _notificationService;

    /// <summary>
    /// 初始化 <see cref="DevicesViewModel"/> 类的新实例。
    /// </summary>
    /// <param name="mapper">对象映射器。</param>
    /// <param name="dataStorageService">数据存储服务。</param>
    /// <param name="dialogService">对话框服务。</param>
    /// <param name="navigationService">导航服务。</param>
    /// <param name="wpfDataService">主数据服务。</param>
    /// <param name="deviceAppService">设备应用服务。</param>
    /// <param name="notificationService">通知服务。</param>
    public DevicesViewModel(IMapper mapper, IDataStorageService dataStorageService,
                            IDialogService dialogService, INavigationService navigationService,
                            IWPFDataService wpfDataService, IDeviceAppService deviceAppService,
                            INotificationService notificationService)
    {
        _mapper = mapper;
        _dataStorageService = dataStorageService;
        _dialogService = dialogService;
        _navigationService = navigationService;
        _wpfDataService = wpfDataService;
        _deviceAppService = deviceAppService;
        _notificationService = notificationService;
        Devices = _dataStorageService.Devices.ToNotifyCollectionChanged(x => x.Value);
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
            DeviceItem device = await _dialogService.ShowDialogAsync(new DeviceDialogViewModel()
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
            // 添加null检查
            if (_mapper != null)
            {
                dto.Device = _mapper.Map<Device>(device);
            }
            else
            {
                _notificationService?.ShowError("映射服务未初始化");
                return;
            }


            dto.DeviceMenu = new MenuBeanDto()
                             {
                                 Header = device.Name,
                                 Icon = SegoeFluentIcons.Devices2.Glyph,
                                 TargetViewKey = nameof(DeviceDetailViewModel),
                             };

            if (device.IsAddDefVarTable)
            {
                dto.VariableTable = new VariableTable()
                                    {
                                        Name = "默认变量表",
                                        Description = "默认变量表",
                                        IsActive = true
                                    };
                dto.VariableTableMenu = new MenuBeanDto()
                                        {
                                            Header = dto.VariableTable.Name,
                                            Icon = SegoeFluentIcons.DataSense.Glyph,
                                            TargetViewKey = nameof(VariableTableViewModel)
                                        };
            }


            // 添加设备
            // 添加null检查
            if (_wpfDataService != null && _wpfDataService.DeviceDataService != null)
            {
                var addDto = await _wpfDataService.DeviceDataService.AddDevice(dto);

                // 添加null检查
                if (addDto != null && addDto.Device != null && _notificationService != null)
                {
                    _notificationService.ShowSuccess($"设备添加成功：{addDto.Device.Name}");
                    
                }
                else if (_notificationService != null)
                {
                    _notificationService.ShowError("设备添加失败");
                }
            }
            else if (_notificationService != null)
            {
                _notificationService.ShowError("数据服务未初始化");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // 添加null检查
            if (_notificationService != null)
            {
                _notificationService.ShowError($"添加设备的过程中发生错误：{e.Message}", e);
            }
        }
    }

    /// <summary>
    /// 删除设备命令。
    /// </summary>
    [RelayCommand]
    private async Task DeleteDevice(DeviceItem parmDeviceItem)
    {
        try
        {
            if (parmDeviceItem != null)
            {
                SelectedDevice = parmDeviceItem;
            }
            if (SelectedDevice == null)
            {
                _notificationService.ShowError("你没有选择任何设备，请选择设备后再点击删除设备");
                return;
            }

            var viewModel = new ConfirmDialogViewModel("删除设备", $"确认要删除设备名为:{SelectedDevice.Name}", "删除设备");
            if (await _dialogService.ShowDialogAsync(viewModel))
            {
                var deviceName = SelectedDevice.Name;
                if (await _wpfDataService.DeviceDataService.DeleteDevice(SelectedDevice))
                {
                    _notificationService.ShowSuccess($"删除设备成功,设备名：{deviceName}");
                }
            }
        }
        catch (Exception e)
        {
            _notificationService.ShowError($"删除设备的过程中发生错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 编辑设备命令。
    /// </summary>
    [RelayCommand]
    private async Task EditDevice(DeviceItem parmDeviceItem)
    {
        try
        {
            if (parmDeviceItem != null)
            {
                SelectedDevice = parmDeviceItem;
            }
            
            if (SelectedDevice == null)
            {
                _notificationService.ShowError("你没有选择任何设备，请选择设备后再点击编辑设备");
                return;
            }

            DeviceDialogViewModel deviceDialogViewModel = new DeviceDialogViewModel(SelectedDevice)
                                                          {
                                                              PrimaryButText = "编辑设备"
                                                          };
            // 1. 显示设备对话框
            DeviceItem device = await _dialogService.ShowDialogAsync(deviceDialogViewModel);
            // 如果用户取消或对话框未返回设备，则直接返回
            if (device == null)
            {
                return;
            }

            if (await _wpfDataService.DeviceDataService.UpdateDevice(device))
            {
                _notificationService.ShowSuccess($"编辑设备成功：{device.Name}");
            }
        }
        catch (Exception e)
        {
            _notificationService.ShowError($"编辑设备的过程中发生错误：{e.Message}", e);
        }
    }

    [RelayCommand]
    public void NavigateToDetail()
    {
        if (SelectedDevice == null) return;

        _navigationService.NavigateToAsync(this,new NavigationParameter(nameof(DeviceDetailViewModel),SelectedDevice.Id,NavigationType.Device));
    }


    [RelayCommand]
    private async Task AddVariableTable(DeviceItem device)
    {
        if (device == null) return;

        try
        {
            VariableTableDialogViewModel variableTableDialogViewModel = new VariableTableDialogViewModel()
                                                                        {
                                                                            PrimaryButText = "添加变量表"
                                                                        };
            // 显示添加变量表对话框
            var VariableTableItem = await _dialogService.ShowDialogAsync(variableTableDialogViewModel);
            // 如果用户取消或对话框未返回变量表，则直接返回
            if (VariableTableItem == null)
            {
                return;
            }

            VariableTableItem.DeviceId = device.Id;
            var tableMenu = new MenuBeanDto()
                            {
                                Header = VariableTableItem.Name,
                                Icon = SegoeFluentIcons.DataSense.Glyph,
                                TargetViewKey = nameof(VariableTableViewModel)
                            };
            int addVarTableId = await _wpfDataService.VariableTableDataService.AddVariableTable(
                _mapper.Map<VariableTable>(VariableTableItem),
                tableMenu, true);

            if (addVarTableId > 0)
            {
                VariableTableItem.Id = addVarTableId;
                if (_dataStorageService.Devices.TryGetValue(VariableTableItem.DeviceId, out var deviceModel))
                {
                    VariableTableItem.Device = deviceModel;
                    deviceModel.VariableTables.Add(VariableTableItem);
                    _dataStorageService.VariableTables.Add(VariableTableItem.Id, VariableTableItem);
                }
                _notificationService.ShowSuccess($"添加变量表成功：{VariableTableItem.Name}");
            }
            else
            {
                _notificationService.ShowError($"添加变量表失败：{VariableTableItem.Name}！！");
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"添加变量表时发生错误: {ex.Message}", ex);
        }
    }

    [RelayCommand]
    private async Task EditVariableTable(VariableTableItem variableTable)
    {
        if (variableTable == null)
        {
            _notificationService.ShowError("你没有选择任何变量表，请选择变量表后再点击编辑变量表");
            return;
        }

        try
        {
            VariableTableDialogViewModel variableTableDialogViewModel
                = new VariableTableDialogViewModel(variableTable)
                  {
                      PrimaryButText = "编辑变量表"
                  };
            // 显示变量表对话框
            VariableTableItem updatedVariableTable
                = await _dialogService.ShowDialogAsync(variableTableDialogViewModel);
            // 如果用户取消或对话框未返回变量表，则直接返回
            if (updatedVariableTable == null)
            {
                return;
            }

            if (await _wpfDataService.VariableDataService.UpdateVariableTable(updatedVariableTable))
            {
                _notificationService.ShowSuccess($"编辑变量表成功：{updatedVariableTable.Name}");
                
                // Update the properties in the original variable table
                variableTable.Name = updatedVariableTable.Name;
                variableTable.Description = updatedVariableTable.Description;
                variableTable.IsActive = updatedVariableTable.IsActive;
            }
            else
            {
                _notificationService.ShowError($"编辑变量表失败：{updatedVariableTable.Name}");
            }
        }
        catch (Exception e)
        {
            _notificationService.ShowError($"编辑变量表的过程中发生错误：{e.Message}", e);
        }
    }

    [RelayCommand]
    private async Task DeleteVariableTable(VariableTableItem variableTable)
    {
        if (variableTable == null)
        {
            _notificationService.ShowError("你没有选择任何变量表，请选择变量表后再点击删除变量表");
            return;
        }

        try
        {
            string message = $"确认要删除变量表名为:{variableTable.Name} \n\n此操作将同时删除该变量表下的所有变量数据，且无法恢复！";
            ConfirmDialogViewModel viewModel = new ConfirmDialogViewModel("删除变量表", message, "删除");
            if (await _dialogService.ShowDialogAsync(viewModel))
            {
                var tableName = variableTable.Name;
                if (await _wpfDataService.VariableDataService.DeleteVariableTable(variableTable, true))
                {
                    // Remove from parent device's collection
                    if (variableTable.Device != null)
                    {
                        if (_dataStorageService.Devices.TryGetValue(variableTable.DeviceId ,out var device))
                        {
                            device.VariableTables.Remove(variableTable);
                        }
                        _dataStorageService.VariableTables.Remove(variableTable.Id);
                        
                        variableTable.Device.VariableTables.Remove(variableTable);
                    }
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

    private void OnDeviceIsActiveChanged(object? sender, bool isActive)
    {
        if (sender is DeviceItem DeviceItem)
        {
        }
    }
}