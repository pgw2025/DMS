using System.Collections.ObjectModel;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
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
    private readonly IMenuDataService _menuDataService;
    private readonly IVariableDataService _variableDataService;

    /// <summary>
    /// DeviceDataService类的构造函数。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例。</param>
    /// <param name="appDataCenterService">数据服务中心实例。</param>
    public DeviceDataService(IMapper mapper, IAppDataCenterService appDataCenterService,IAppDataStorageService appDataStorageService, IDataStorageService dataStorageService,IMenuDataService menuDataService,IVariableDataService variableDataService)
    {
        _mapper = mapper;
        _appDataCenterService = appDataCenterService;
        _appDataStorageService = appDataStorageService;
        _dataStorageService = dataStorageService;
        _menuDataService = menuDataService;
        _variableDataService = variableDataService;
    }

    /// <summary>
    /// 加载所有设备数据。
    /// </summary>
    public void LoadAllDevices()
    {
        foreach (var deviceDto in _appDataStorageService.Devices.Values)
        {
            _dataStorageService.Devices.Add(_mapper.Map<DeviceItemViewModel>(deviceDto));
        }
        
    }

    /// <summary>
    /// 添加设备。
    /// </summary>
    public async Task<CreateDeviceWithDetailsDto> AddDevice(CreateDeviceWithDetailsDto dto)
    {
        var addDto = await _appDataCenterService.DeviceManagementService.CreateDeviceWithDetailsAsync(dto);
        //更新当前界面
        _dataStorageService.Devices.Add(_mapper.Map<DeviceItemViewModel>(addDto.Device));
        _menuDataService.AddMenuItem(_mapper.Map<MenuItemViewModel>(addDto.DeviceMenu));
        await _variableDataService.AddVariableTable(addDto.VariableTable);
        _menuDataService.AddMenuItem(_mapper.Map<MenuItemViewModel>(addDto.VariableTableMenu));
        //更新数据中心
        _appDataCenterService.DeviceManagementService.AddDeviceToMemory(addDto.Device);
        _appDataCenterService.VariableTableManagementService.AddVariableTableToMemory(addDto.VariableTable);
        _appDataCenterService.MenuManagementService.AddMenuToMemory(addDto.DeviceMenu);
        _appDataCenterService.MenuManagementService.AddMenuToMemory(addDto.VariableTableMenu);

        _menuDataService.BuildMenuTrees();


        return addDto;
    }

    /// <summary>
    /// 删除设备。
    /// </summary>
    public async Task<bool> DeleteDevice(DeviceItemViewModel device)
    {
        if (!await _appDataCenterService.DeviceManagementService.DeleteDeviceByIdAsync(device.Id))
        {
            return false;
        }

        _appDataCenterService.DeviceManagementService.RemoveDeviceFromMemory(device.Id);

        // 删除设备
        _dataStorageService.Devices.Remove(device);

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