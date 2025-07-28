using AutoMapper;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Interfaces;

namespace DMS.Application.Services;

/// <summary>
/// 设备应用服务，负责处理设备相关的业务逻辑。
/// 实现 <see cref="IDeviceAppService"/> 接口。
/// </summary>
public class DeviceAppService : IDeviceAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，通过依赖注入获取仓储管理器和AutoMapper实例。
    /// </summary>
    /// <param name="repoManager">仓储管理器实例。</param>
    /// <param name="mapper">AutoMapper 实例。</param>
    public DeviceAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取设备数据传输对象。
    /// </summary>
    /// <param name="id">设备ID。</param>
    /// <returns>设备数据传输对象。</returns>
    public async Task<DeviceDto> GetDeviceByIdAsync(int id)
    {
        var device = await _repoManager.Devices.GetByIdAsync(id);
        return _mapper.Map<DeviceDto>(device);
    }

    /// <summary>
    /// 异步获取所有设备数据传输对象列表。
    /// </summary>
    /// <returns>设备数据传输对象列表。</returns>
    public async Task<List<DeviceDto>> GetAllDevicesAsync()
    {
        var devices = await _repoManager.Devices.GetAllAsync();
        return _mapper.Map<List<DeviceDto>>(devices);
    }

    /// <summary>
    /// 异步创建一个新设备及其关联的变量表和菜单（事务性操作）。
    /// </summary>
    /// <param name="dto">包含设备、变量表和菜单信息的创建数据传输对象。</param>
    /// <returns>新创建设备的ID。</returns>
    /// <exception cref="InvalidOperationException">如果添加设备、设备菜单或变量表失败。</exception>
    /// <exception cref="ApplicationException">如果创建设备时发生其他错误。</exception>
    public async Task<CreateDeviceWithDetailsDto> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto)
    {
        try
        {
            await _repoManager.BeginTranAsync();

            var device = _mapper.Map<Device>(dto.Device);
            var addDevice = await _repoManager.Devices.AddAsync(device);
            if (addDevice == null || addDevice.Id == 0)
            {
                throw new InvalidOperationException($"添加设备失败：{addDevice}");
            }

            _mapper.Map(addDevice,dto.Device);
            MenuBean addDeviceMenu = null;

            // 假设有设备菜单
            if (dto.DeviceMenu != null)
            {
                var deviceMenu = _mapper.Map<MenuBean>(dto.DeviceMenu);
                deviceMenu.ParentId = 2; // 假设父菜单ID为2
                deviceMenu.MenuType = MenuType.DeviceMenu;
                deviceMenu.TargetId = addDevice.Id;
                addDeviceMenu = await _repoManager.Menus.AddAsync(deviceMenu);
                if (addDeviceMenu == null || addDeviceMenu.Id == 0)
                {
                    throw new InvalidOperationException($"添加设备菜单失败：{addDeviceMenu}");
                }
                _mapper.Map(addDeviceMenu,dto.DeviceMenu);
            }


            // 假设 CreateDeviceWithDetailsDto 包含了变量表和菜单信息
            if (dto.VariableTable != null)
            {
                var variableTable = _mapper.Map<VariableTable>(dto.VariableTable);
                variableTable.DeviceId = device.Id; // 关联新设备ID
                variableTable.Protocol = device.Protocol;
                var addVariableTable = await _repoManager.VariableTables.AddAsync(variableTable);
                if (addVariableTable == null || addVariableTable.Id == 0)
                {
                    throw new InvalidOperationException($"添加设备变量表失败,设备：{device.Name},变量表：{variableTable.Name}");
                }
                 _mapper.Map(addVariableTable,dto.VariableTable);

                // 假设有设备菜单
                if (dto.VariableTableMenu != null)
                {
                    var menu = _mapper.Map<MenuBean>(dto.VariableTableMenu);
                    menu.ParentId = addDeviceMenu.Id; // 关联设备菜单作为父级
                    menu.MenuType = MenuType.VariableTableMenu;
                    menu.TargetId = addVariableTable.Id;
                    var addVariableTableMenu = await _repoManager.Menus.AddAsync(menu);
                    if (addVariableTableMenu == null || addVariableTableMenu.Id == 0)
                    {
                        throw new InvalidOperationException(
                            $"添加设备变量表菜单失败,变量表：{variableTable.Name},变量表菜单：{menu.Header}");
                    }
                    _mapper.Map(menu,dto.VariableTableMenu);
                }
            }

            await _repoManager.CommitAsync();

            return dto;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            // 可以在此记录日志
            throw new ApplicationException($"创建设备时发生错误，操作已回滚,错误信息:{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步更新一个已存在的设备。
    /// </summary>
    /// <param name="deviceDto">要更新的设备数据传输对象。</param>
    /// <returns>受影响的行数。</returns>
    /// <exception cref="ApplicationException">如果找不到设备。</exception>
    public async Task<int> UpdateDeviceAsync(UpdateDeviceDto deviceDto)
    {
        await _repoManager.BeginTranAsync();
        var device = await _repoManager.Devices.GetByIdAsync(deviceDto.Id);
        if (device == null)
        {
            throw new ApplicationException($"Device with ID {deviceDto.Id} not found.");
        }

        _mapper.Map(deviceDto, device);
        int res=await _repoManager.Devices.UpdateAsync(device);
        await _repoManager.CommitAsync();
        return res;
    }

    /// <summary>
    /// 异步删除一个设备。
    /// </summary>
    /// <param name="device">要删除的设备实体。</param>
    /// <returns>表示异步操作的任务。</returns>
    public async Task DeleteDeviceAsync(Device device)
    {
       await DeleteDeviceByIdAsync(device.Id);
    }

    /// <summary>
    /// 异步根据ID删除一个设备，包括其关联的变量表和菜单（事务性操作）。
    /// </summary>
    /// <param name="deviceId">要删除设备的ID。</param>
    /// <returns>如果删除成功则为 true，否则为 false。</returns>
    /// <exception cref="InvalidOperationException">如果删除设备失败。</exception>
    /// <exception cref="ApplicationException">如果删除设备时发生其他错误。</exception>
    public async Task<bool> DeleteDeviceByIdAsync(int deviceId)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var delRes = await _repoManager.Devices.DeleteByIdAsync(deviceId);
            if (delRes == 0)
            {
                throw new InvalidOperationException($"删除设备失败：设备ID:{deviceId}，请检查设备Id是否存在");
            }

            // 删除关联的变量表
            await _repoManager.VariableTables.DeleteByDeviceIdAsync(deviceId);
            
            // 删除关联的菜单树
            await _repoManager.Menus.DeleteMenuTreeByTargetIdAsync(MenuType.DeviceMenu,deviceId);

            await _repoManager.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            // 可以在此记录日志
            throw new ApplicationException($"删除设备时发生错误，操作已回滚,错误信息:{ex.Message}", ex);
        }

    }

    /// <summary>
    /// 异步切换设备的激活状态。
    /// </summary>
    /// <param name="id">设备的ID。</param>
    /// <returns>表示异步操作的任务。</returns>
    /// <exception cref="ApplicationException">如果找不到设备。</exception>
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
    /// <param name="protocol">协议类型。</param>
    /// <returns>设备数据传输对象列表。</returns>
    public async Task<List<DeviceDto>> GetDevicesByProtocolAsync(ProtocolType protocol)
    {
        var devices = await _repoManager.Devices.GetAllAsync();
        return _mapper.Map<List<DeviceDto>>(devices);
    }
}