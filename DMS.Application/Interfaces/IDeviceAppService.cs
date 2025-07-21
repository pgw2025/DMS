using DMS.Application.DTOs;
using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义设备管理相关的应用服务操作。
/// </summary>
public interface IDeviceAppService
{
    /// <summary>
    /// 异步根据ID获取设备DTO。
    /// </summary>
    Task<DeviceDto> GetDeviceByIdAsync(int id);

    /// <summary>
    /// 异步获取所有设备DTO列表。
    /// </summary>
    Task<List<DeviceDto>> GetAllDevicesAsync();

    /// <summary>
    /// 异步创建一个新设备及其关联的变量表和菜单（事务性操作）。
    /// </summary>
    /// <param name="dto">包含设备、变量表和菜单信息的DTO。</param>
    /// <returns>新创建设备的ID。</returns>
    Task<int> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto);

    /// <summary>
    /// 异步更新一个已存在的设备。
    /// </summary>
    Task UpdateDeviceAsync(UpdateDeviceDto deviceDto);

    /// <summary>
    /// 异步删除一个设备。
    /// </summary>
    Task DeleteDeviceAsync(Device device);

    /// <summary>
    /// 异步切换设备的激活状态。
    /// </summary>
    Task ToggleDeviceActiveStateAsync(int id);

    /// <summary>
    /// 异步获取指定协议类型的设备列表。
    /// </summary>
    Task<List<DeviceDto>> GetDevicesByProtocolAsync(ProtocolType protocol);
}