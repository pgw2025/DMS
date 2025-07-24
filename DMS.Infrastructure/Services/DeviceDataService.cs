using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services;

public class DeviceDataService : IDeviceDataService
{
    private readonly IRepositoryManager _repositoryManager;
    private List<Device> _devices;

    public List<Device> Devices => _devices;

    public event Action<List<Device>> OnDeviceListChanged;
    public event Action<Device, bool> OnDeviceIsActiveChanged;

    public DeviceDataService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
        _devices = new List<Device>();
    }

    public async Task InitializeAsync()
    {
        await LoadDevicesAsync();
    }

    private async Task LoadDevicesAsync()
    {
        _devices = (await _repositoryManager.Devices.GetAllAsync()).ToList();
        OnDeviceListChanged?.Invoke(_devices);
    }

    // 模拟设备激活状态变更，实际应用中可能由其他服务触发
    public async Task SetDeviceIsActiveAsync(int deviceId, bool isActive)
    {
        var device = _devices.FirstOrDefault(d => d.Id == deviceId);
        if (device != null)
        {
            device.IsActive = isActive;
            OnDeviceIsActiveChanged?.Invoke(device, isActive);
            // 实际应用中，这里可能还需要更新数据库
            await _repositoryManager.Devices.UpdateAsync(device);
        }
    }

    // 模拟设备列表变更，实际应用中可能由其他服务触发
    public async Task RefreshDeviceListAsync()
    {
        await LoadDevicesAsync();
    }
}