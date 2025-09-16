using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;

namespace DMS.Application.Services.Management;

/// <summary>
/// 设备管理服务，负责设备相关的业务逻辑。
/// </summary>
public class DeviceManagementService : IDeviceManagementService
{
    private readonly IDeviceAppService _deviceAppService;
    private readonly IAppDataStorageService _appDataStorageService;

    /// <summary>
    /// 当设备数据发生变化时触发
    /// </summary>
    public event EventHandler<DeviceChangedEventArgs> OnDeviceChanged;

    public DeviceManagementService(IDeviceAppService deviceAppService,IAppDataStorageService appDataStorageService)
    {
        _deviceAppService = deviceAppService;
        _appDataStorageService = appDataStorageService;
    }

    /// <summary>
    /// 异步根据ID获取设备DTO。
    /// </summary>
    public async Task<DeviceDto> GetDeviceByIdAsync(int id)
    {
        return await _deviceAppService.GetDeviceByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有设备DTO列表。
    /// </summary>
    public async Task<List<DeviceDto>> GetAllDevicesAsync()
    {
        return await _deviceAppService.GetAllDevicesAsync();
    }

    /// <summary>
    /// 异步创建一个新设备及其关联的变量表和菜单（事务性操作）。
    /// </summary>
    public async Task<CreateDeviceWithDetailsDto> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto)
    {
        return await _deviceAppService.CreateDeviceWithDetailsAsync(dto);
    }

    /// <summary>
    /// 异步更新一个已存在的设备。
    /// </summary>
    public async Task<int> UpdateDeviceAsync(DeviceDto deviceDto)
    {
        return await _deviceAppService.UpdateDeviceAsync(deviceDto);
    }

    /// <summary>
    /// 异步删除一个设备。
    /// </summary>
    public async Task<bool> DeleteDeviceByIdAsync(int deviceId)
    {
        return await _deviceAppService.DeleteDeviceByIdAsync(deviceId);
    }

    /// <summary>
    /// 异步切换设备的激活状态。
    /// </summary>
    public async Task ToggleDeviceActiveStateAsync(int id)
    {
        await _deviceAppService.ToggleDeviceActiveStateAsync(id);
    }

    /// <summary>
    /// 在内存中添加设备
    /// </summary>
    public void AddDeviceToMemory(DeviceDto deviceDto)
    {
        if (_appDataStorageService.Devices.TryAdd(deviceDto.Id, deviceDto))
        {
            OnDeviceChanged?.Invoke(this,new DeviceChangedEventArgs(DataChangeType.Added, deviceDto));
        }
    }

    /// <summary>
    /// 在内存中更新设备
    /// </summary>
    public void UpdateDeviceInMemory(DeviceDto deviceDto)
    {
        _appDataStorageService.Devices.AddOrUpdate(deviceDto.Id, deviceDto, (key, oldValue) => deviceDto);
        OnDeviceChanged?.Invoke(this,new DeviceChangedEventArgs(DataChangeType.Updated, deviceDto));
    }

    /// <summary>
    /// 在内存中删除设备
    /// </summary>
    public void RemoveDeviceFromMemory(int deviceId)
    {
        if (_appDataStorageService.Devices.TryGetValue(deviceId, out var deviceDto))
        {
            foreach (var variableTable in deviceDto.VariableTables)
            {
                foreach (var variable in variableTable.Variables)
                {
                    _appDataStorageService.Variables.TryRemove(variable.Id, out _);
                }

                _appDataStorageService.VariableTables.TryRemove(variableTable.Id, out _);
            }

            _appDataStorageService.Devices.TryRemove(deviceId, out _);

            OnDeviceChanged?.Invoke(this,new DeviceChangedEventArgs(DataChangeType.Deleted, deviceDto));
        }
    }


}