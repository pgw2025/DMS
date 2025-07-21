using AutoMapper;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Interfaces;

namespace DMS.Application.Services;

/// <summary>
/// 实现设备管理的应用服务。
/// </summary>
public class DeviceAppService : IDeviceAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，通过依赖注入获取仓储管理器和AutoMapper实例。
    /// </summary>
    public DeviceAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取设备DTO。
    /// </summary>
    public async Task<DeviceDto> GetDeviceByIdAsync(int id)
    {
        var device = await _repoManager.Devices.GetByIdAsync(id);
        return _mapper.Map<DeviceDto>(device);
    }

    /// <summary>
    /// 异步获取所有设备DTO列表。
    /// </summary>
    public async Task<List<DeviceDto>> GetAllDevicesAsync()
    {
        var devices = await _repoManager.Devices.GetAllAsync();
        return _mapper.Map<List<DeviceDto>>(devices);
    }

    /// <summary>
    /// 异步创建一个新设备及其关联的变量表和菜单（事务性操作）。
    /// </summary>
    public async Task<int> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto)
    {
        try
        {
            _repoManager.BeginTranAsync();

            var device = _mapper.Map<Device>(dto.Device);
            device.IsActive = true; // 默认激活
            await _repoManager.Devices.AddAsync(device);

            // 假设 CreateDeviceWithDetailsDto 包含了变量表和菜单信息
            if (dto.VariableTable != null)
            {
                var variableTable = _mapper.Map<VariableTable>(dto.VariableTable);
                variableTable.DeviceId = device.Id; // 关联新设备ID
                await _repoManager.VariableTables.AddAsync(variableTable);
            }

            // 假设有菜单服务或仓储
            // if (dto.Menu != null)
            // {
            //     var menu = _mapper.Map<Menu>(dto.Menu);
            //     menu.TargetId = device.Id;
            //     await _repoManager.Menus.AddAsync(menu);
            // }

            await _repoManager.CommitAsync();

            return device.Id;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            // 可以在此记录日志
            throw new ApplicationException("创建设备时发生错误，操作已回滚。", ex);
        }
    }

    /// <summary>
    /// 异步更新一个已存在的设备。
    /// </summary>
    public async Task UpdateDeviceAsync(UpdateDeviceDto deviceDto)
    {
        var device = await _repoManager.Devices.GetByIdAsync(deviceDto.Id);
        if (device == null)
        {
            throw new ApplicationException($"Device with ID {deviceDto.Id} not found.");
        }
        _mapper.Map(deviceDto, device);
        await _repoManager.Devices.UpdateAsync(device);
        await _repoManager.CommitAsync();
    }

    /// <summary>
    /// 异步删除一个设备。
    /// </summary>
    public async Task DeleteDeviceAsync(Device device)
    {
        await _repoManager.Devices.DeleteAsync(device);
        await _repoManager.CommitAsync();
    }

    /// <summary>
    /// 异步切换设备的激活状态。
    /// </summary>
    public async Task ToggleDeviceActiveStateAsync(int id)
    {
        var device = await _repoManager.Devices.GetByIdAsync(id);
        if (device == null)
        {
            throw new ApplicationException($"Device with ID {id} not found.");
        }
        device.IsActive = !device.IsActive;
        await _repoManager.Devices.UpdateAsync(device);
        await _repoManager.CommitAsync();
    }

    /// <summary>
    /// 异步获取指定协议类型的设备列表。
    /// </summary>
    public async Task<List<DeviceDto>> GetDevicesByProtocolAsync(ProtocolType protocol)
    {
        var devices = await _repoManager.Devices.GetAllAsync();
        return _mapper.Map<List<DeviceDto>>(devices);
    }
}