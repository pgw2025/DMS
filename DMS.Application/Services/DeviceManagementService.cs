using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Core.Models;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Core.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace DMS.Application.Services;

/// <summary>
/// 设备管理服务，负责设备相关的业务逻辑。
/// </summary>
public class DeviceManagementService
{
    private readonly IDeviceAppService _deviceAppService;
    private readonly ConcurrentDictionary<int, DeviceDto> _devices;

    /// <summary>
    /// 当设备数据发生变化时触发
    /// </summary>
    public event EventHandler<DeviceChangedEventArgs> DeviceChanged;

    public DeviceManagementService(IDeviceAppService deviceAppService, ConcurrentDictionary<int, DeviceDto> devices)
    {
        _deviceAppService = deviceAppService;
        _devices = devices;
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
    public void AddDeviceToMemory(DeviceDto deviceDto, ConcurrentDictionary<int, VariableTableDto> variableTables, 
                                  ConcurrentDictionary<int, VariableDto> variables)
    {
        if (_devices.TryAdd(deviceDto.Id, deviceDto))
        {
            OnDeviceChanged(new DeviceChangedEventArgs(DataChangeType.Added, deviceDto));
        }
    }

    /// <summary>
    /// 在内存中更新设备
    /// </summary>
    public void UpdateDeviceInMemory(DeviceDto deviceDto)
    {
        _devices.AddOrUpdate(deviceDto.Id, deviceDto, (key, oldValue) => deviceDto);
        OnDeviceChanged(new DeviceChangedEventArgs(DataChangeType.Updated, deviceDto));
    }

    /// <summary>
    /// 在内存中删除设备
    /// </summary>
    public void RemoveDeviceFromMemory(int deviceId, ConcurrentDictionary<int, VariableTableDto> variableTables,
                                       ConcurrentDictionary<int, VariableDto> variables)
    {
        if (_devices.TryGetValue(deviceId, out var deviceDto))
        {
            foreach (var variableTable in deviceDto.VariableTables)
            {
                foreach (var variable in variableTable.Variables)
                {
                    variables.TryRemove(variable.Id, out _);
                }

                variableTables.TryRemove(variableTable.Id, out _);
            }

            _devices.TryRemove(deviceId, out _);

            OnDeviceChanged(new DeviceChangedEventArgs(DataChangeType.Deleted, deviceDto));
        }
    }

    /// <summary>
    /// 触发设备变更事件
    /// </summary>
    protected virtual void OnDeviceChanged(DeviceChangedEventArgs e)
    {
        DeviceChanged?.Invoke(this, e);
    }
}