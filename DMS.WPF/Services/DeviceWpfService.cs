using System.Collections.ObjectModel;
using System.Windows.Threading;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Events;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.Services;

/// <summary>
/// 设备数据服务类，负责管理设备相关的数据和操作。
/// </summary>
public class DeviceWpfService : IDeviceDataService
{
    private readonly IMapper _mapper;
    private readonly IAppCenterService _appCenterService;
    private readonly IAppStorageService _appStorageService;
    private readonly IWpfDataService _dataStorageService;
    private readonly IVariableTableDataService _variableTableDataService;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;
    private readonly IMenuWpfService _menuDataService;
    private readonly IVariableDataService _variableDataService;
    private readonly Dispatcher _uiDispatcher;

    /// <summary>
    /// DeviceDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="appCenterService">数据服务中心实例。</param>
    public DeviceWpfService(IMapper mapper, IAppCenterService appCenterService,
                             IAppStorageService appStorageService, IWpfDataService dataStorageService, IVariableTableDataService variableTableDataService,
                             IEventService eventService, INotificationService notificationService,
                             IMenuWpfService menuDataService, IVariableDataService variableDataService)
    {
       
        _mapper = mapper;
        _appCenterService = appCenterService;
        _appStorageService = appStorageService;
        _dataStorageService = dataStorageService;
        _variableTableDataService = variableTableDataService;
        _eventService = eventService;
        _notificationService = notificationService;
        _menuDataService = menuDataService;
        _variableDataService = variableDataService;
        _uiDispatcher = Dispatcher.CurrentDispatcher;

        _eventService.OnDeviceStateChanged += OnDeviceStateChanged;
    }

    private void OnDeviceStateChanged(object? sender, DeviceStateChangedEventArgs e)
    {
        // 只处理连接状态变化
        if (e.StateType == Core.Enums.DeviceStateType.Connection)
        {
            _uiDispatcher.Invoke(() =>
            {

                if (_dataStorageService.Devices.TryGetValue(e.DeviceId, out DeviceItem device))
                {

                    device.IsRunning = e.StateValue;
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
    }

    /// <summary>
    /// 加载所有设备数据。
    /// </summary>
    public void LoadAllDevices()
    {
        foreach (var device in _appStorageService.Devices.Values)
        {
            _dataStorageService.Devices.Add(device.Id, _mapper.Map<DeviceItem>(device));
        }
    }

    /// <summary>
    /// 添加设备。
    /// </summary>
    public async Task<CreateDeviceWithDetailsDto?> AddDevice(CreateDeviceWithDetailsDto dto)
    {
        // 添加null检查
        if (dto is null) return null;

        var addDto = await _appCenterService.DeviceManagementService.CreateDeviceWithDetailsAsync(dto);

        // 添加null检查
        if (addDto is null) return null;

        //给界面添加设备
        _dataStorageService.Devices.Add(addDto.Device.Id, _mapper.Map<DeviceItem>(addDto.Device));

        // 给界面添加设备菜单
        if (addDto.DeviceMenu != null)
        {
            _menuDataService.AddMenuToView(_mapper.Map<MenuItem>(addDto.DeviceMenu));

        }


        // 添加变量表和变量表菜单
        if (addDto.VariableTable != null)
        {
            await _variableDataService.AddVariableTableToView(addDto.VariableTable);
            // 添加变量表到内存的操作现在在服务内部完成，无需额外调用

            if (addDto.VariableTable != null && addDto.VariableTableMenu != null)
            {
                _menuDataService.AddMenuToView(_mapper.Map<MenuItem>(addDto.VariableTableMenu));
            }


        }



        return addDto;
    }

    /// <summary>
    /// 删除设备。
    /// </summary>
    public async Task<bool> DeleteDevice(DeviceItem device)
    {

        //从数据库和内存中删除设备相关数据
        if (!await _appCenterService.DeviceManagementService.DeleteDeviceByIdAsync(device.Id))
        {
            return false;
        }


        // 从界面删除设备相关数据集
        var variableTablesCopy = device.VariableTables.ToList();
        foreach (var variableTable in variableTablesCopy)
        {
            await _variableTableDataService.DeleteVariableTable(variableTable);
        }

        var deviceMenu = _dataStorageService.Menus.FirstOrDefault(m => m.MenuType == MenuType.DeviceMenu && m.TargetId == device.Id);
        if (deviceMenu != null)
        {
            await _menuDataService.DeleteMenuItem(deviceMenu);
        }
        _dataStorageService.Devices.Remove(device.Id);


        return true;
    }

    /// <summary>
    /// 更新设备。
    /// </summary>
    public async Task<bool> UpdateDevice(DeviceItem device)
    {
        if (!_appStorageService.Devices.TryGetValue(device.Id, out var existingDevice))
        {
            return false;
        }

        _mapper.Map(device, existingDevice);
        if (await _appCenterService.DeviceManagementService.UpdateDeviceAsync(existingDevice) > 0)
        {
            // 更新数据库后会自动更新内存，无需额外操作
            return true;
        }

        return false;
    }
}