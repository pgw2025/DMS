using System.Collections.ObjectModel;
using System.Windows.Threading;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Events;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Services;

/// <summary>
/// 设备数据服务类，负责管理设备相关的数据和操作。
/// </summary>
public class DeviceDataService : IDeviceDataService
{
    private readonly IMapper _mapper;
    private readonly IAppDataCenterService _appDataCenterService;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly IDataStorageService _dataStorageService;
    private readonly IVariableTableDataService _variableTableDataService;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;
    private readonly IMenuDataService _menuDataService;
    private readonly IVariableDataService _variableDataService;
    private readonly Dispatcher _uiDispatcher;

    /// <summary>
    /// DeviceDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="appDataCenterService">数据服务中心实例。</param>
    public DeviceDataService(IMapper mapper, IAppDataCenterService appDataCenterService,
                             IAppDataStorageService appDataStorageService, IDataStorageService dataStorageService,IVariableTableDataService variableTableDataService,
                             IEventService eventService, INotificationService notificationService,
                             IMenuDataService menuDataService, IVariableDataService variableDataService)
    {
        _mapper = mapper;
        _appDataCenterService = appDataCenterService;
        _appDataStorageService = appDataStorageService;
        _dataStorageService = dataStorageService;
        _variableTableDataService = variableTableDataService;
        _eventService = eventService;
        _notificationService = notificationService;
        _menuDataService = menuDataService;
        _variableDataService = variableDataService;
        _uiDispatcher = Dispatcher.CurrentDispatcher;

        _eventService.OnDeviceConnectChanged += OnDeviceConnectChanged;
    }

    private void OnDeviceConnectChanged(object? sender, DeviceConnectChangedEventArgs e)
    {
        _uiDispatcher.Invoke(() =>
        {

            if (_dataStorageService.Devices.TryGetValue(e.DeviceId, out DeviceItemViewModel device))
            {

                device.IsRunning = e.NewStatus;
                if (device.IsRunning)
                {
                    _notificationService.ShowSuccess($"设备：{device.Name},连接成功。");
                }
                else
                {
                    _notificationService.ShowSuccess($"设备：{device.Name},已断开连接。");
                }
            }
        });
    }

    /// <summary>
    /// 加载所有设备数据。
    /// </summary>
    public void LoadAllDevices()
    {
        foreach (var deviceDto in _appDataStorageService.Devices.Values)
        {
            _dataStorageService.Devices.Add(deviceDto.Id, _mapper.Map<DeviceItemViewModel>(deviceDto));
        }
    }

    /// <summary>
    /// 添加设备。
    /// </summary>
    public async Task<CreateDeviceWithDetailsDto> AddDevice(CreateDeviceWithDetailsDto dto)
    {
        // 添加null检查
        if (dto == null)
            return null;

        var addDto = await _appDataCenterService.DeviceManagementService.CreateDeviceWithDetailsAsync(dto);

        // 添加null检查
        if (addDto == null && addDto.Device == null)
        {
            return null;
        }

        //给界面添加设备
        _dataStorageService.Devices.Add(addDto.Device.Id, _mapper.Map<DeviceItemViewModel>(addDto.Device));
        //更新数据中心
        _appDataCenterService.DeviceManagementService.AddDeviceToMemory(addDto.Device);

        // 给界面添加设备菜单
        if (addDto.DeviceMenu != null)
        {
            _menuDataService.AddMenuItem(_mapper.Map<MenuItemViewModel>(addDto.DeviceMenu));
            _appDataCenterService.MenuManagementService.AddMenuToMemory(addDto.DeviceMenu);
            
        }


        // 添加变量表和变量表菜单
        if (addDto.VariableTable != null)
        {
            await _variableDataService.AddVariableTableToView(addDto.VariableTable);
            _appDataCenterService.VariableTableManagementService.AddVariableTableToMemory(addDto.VariableTable);

            if (addDto.VariableTable != null && addDto.VariableTableMenu != null)
            {
                _menuDataService.AddMenuItem(_mapper.Map<MenuItemViewModel>(addDto.VariableTableMenu));
                _appDataCenterService.MenuManagementService.AddMenuToMemory(addDto.VariableTableMenu);
            }


        }


        // 添加null检查
        _menuDataService.BuildMenuTrees();

        return addDto;
    }

    /// <summary>
    /// 删除设备。
    /// </summary>
    public async Task<bool> DeleteDevice(DeviceItemViewModel device)
    {
        
        //从数据库删除设备相关数据
        if (!await _appDataCenterService.DeviceManagementService.DeleteDeviceByIdAsync(device.Id))
        {
            return false;
        }
        //从Application项目删除设备相关数据
        _appDataCenterService.DeviceManagementService.RemoveDeviceFromMemory(device.Id);
        

        // 从界面删除设备相关数据集
        foreach (var variableTable in device.VariableTables)
        {
            await  _variableTableDataService.DeleteVariableTable(variableTable);
        }

        var deviceMenu= _dataStorageService.Menus.FirstOrDefault(m => m.MenuType == MenuType.DeviceMenu && m.TargetId == device.Id);
        if (deviceMenu != null)
        {
            _menuDataService.DeleteMenuItem(deviceMenu);
        }
        _dataStorageService.Devices.Remove(device.Id);

        return true;
    }

    /// <summary>
    /// 更新设备。
    /// </summary>
    public async Task<bool> UpdateDevice(DeviceItemViewModel device)
    {
        if (!_appDataStorageService.Devices.TryGetValue(device.Id, out var deviceDto))
        {
            return false;
        }

        _mapper.Map(device, deviceDto);
        if (await _appDataCenterService.DeviceManagementService.UpdateDeviceAsync(deviceDto) > 0)
        {
            return true;
        }

        return false;
    }
}