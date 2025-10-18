using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Application.Services.Management;

/// <summary>
/// 设备管理服务，负责设备相关的业务逻辑。
/// </summary>
public class DeviceManagementService : IDeviceManagementService
{
    private readonly IDeviceAppService _deviceAppService;
    private readonly IAppStorageService _appStorageService;
    private readonly IEventService _eventService;

    public DeviceManagementService(IDeviceAppService deviceAppService, IAppStorageService appStorageService, IEventService eventService)
    {
        _deviceAppService = deviceAppService;
        _appStorageService = appStorageService;
        _eventService = eventService;
    }

    /// <summary>
    /// 异步根据ID获取设备DTO。
    /// </summary>
    public async Task<Device> GetDeviceByIdAsync(int id)
    {
        return await _deviceAppService.GetDeviceByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有设备DTO列表。
    /// </summary>
    public async Task<List<Device>> GetAllDevicesAsync()
    {
        return await _deviceAppService.GetAllDevicesAsync();
    }

    /// <summary>
    /// 异步创建一个新设备及其关联的变量表和菜单（事务性操作）。
    /// </summary>
    public async Task<CreateDeviceWithDetailsDto> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto)
    {
        var result = await _deviceAppService.CreateDeviceWithDetailsAsync(dto);
        
        // 创建成功后，将设备添加到内存中
        if (result?.Device != null)
        {
            if (_appStorageService.Devices.TryAdd(result.Device.Id, result.Device))
            {
                _eventService.RaiseDeviceChanged(this, new DeviceChangedEventArgs(DataChangeType.Added, result.Device));
            }
            if (_appStorageService.VariableTables.TryAdd(result.VariableTable.Id, result.VariableTable))
            {
                _eventService.RaiseVariableTableChanged(this, new VariableTableChangedEventArgs(DataChangeType.Added, result.VariableTable));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步更新一个已存在的设备。
    /// </summary>
    public async Task<int> UpdateDeviceAsync(Device device)
    {
        var result = await _deviceAppService.UpdateDeviceAsync(device);
        
        // 更新成功后，更新内存中的设备
        if (result > 0 && device != null)
        {
            _appStorageService.Devices.AddOrUpdate(device.Id, device, (key, oldValue) => device);
            _eventService.RaiseDeviceChanged(this, new DeviceChangedEventArgs(DataChangeType.Updated, device));
        }
        
        return result;
    }

    /// <summary>
    /// 异步删除一个设备。
    /// </summary>
    public async Task<bool> DeleteDeviceByIdAsync(int deviceId)
    {
        var device = await _deviceAppService.GetDeviceByIdAsync(deviceId); // 获取设备信息用于内存删除
        var result = await _deviceAppService.DeleteDeviceByIdAsync(deviceId);
        
        // 删除成功后，从内存中移除设备
        if (result && device != null)
        {
            if (_appStorageService.Devices.TryGetValue(deviceId, out var deviceInStorage))
            {
                foreach (var variableTable in deviceInStorage.VariableTables)
                {
                    foreach (var variable in variableTable.Variables)
                    {
                        _appStorageService.Variables.TryRemove(variable.Id, out _);
                    }

                    _appStorageService.VariableTables.TryRemove(variableTable.Id, out _);
                }

                _appStorageService.Devices.TryRemove(deviceId, out _);

                _eventService.RaiseDeviceChanged(this, new DeviceChangedEventArgs(DataChangeType.Deleted, deviceInStorage));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步切换设备的激活状态。
    /// </summary>
    public async Task ToggleDeviceActiveStateAsync(int id)
    {
        await _deviceAppService.ToggleDeviceActiveStateAsync(id);
        
        // 更新内存中的设备状态
        var device = await _deviceAppService.GetDeviceByIdAsync(id);
        if (device != null)
        {
            _appStorageService.Devices.AddOrUpdate(device.Id, device, (key, oldValue) => device);
            _eventService.RaiseDeviceChanged(this, new DeviceChangedEventArgs(DataChangeType.Updated, device));
        }
    }

    /// <summary>
    /// 异步加载所有设备数据到内存中。
    /// </summary>
    public async Task LoadAllDevicesAsync()
    {
        _appStorageService.Devices.Clear();
        var devices = await _deviceAppService.GetAllDevicesAsync();
        
        // 建立设备与变量表的关联
        foreach (var device in devices)
        {
            // 将设备添加到安全字典
            _appStorageService.Devices.TryAdd(device.Id, device);
        }
    }

}