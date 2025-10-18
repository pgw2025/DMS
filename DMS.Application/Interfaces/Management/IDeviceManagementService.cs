using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Core.Models;

namespace DMS.Application.Interfaces.Management;

public interface IDeviceManagementService
{


    /// <summary>
    /// 异步根据ID获取设备DTO。
    /// </summary>
            Task<Device> GetDeviceByIdAsync(int id);
    /// <summary>
    /// 异步获取所有设备DTO列表。
    /// </summary>
            Task<List<Device>> GetAllDevicesAsync();
    /// <summary>
    /// 异步创建一个新设备及其关联的变量表和菜单（事务性操作）。
    /// </summary>
    Task<CreateDeviceWithDetailsDto> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto);

    /// <summary>
    /// 异步更新一个已存在的设备。
    /// </summary>
            Task<int> UpdateDeviceAsync(Device device);
    /// <summary>
    /// 异步删除一个设备。
    /// </summary>
    Task<bool> DeleteDeviceByIdAsync(int deviceId);

    /// <summary>
    /// 异步切换设备的激活状态。
    /// </summary>
    Task ToggleDeviceActiveStateAsync(int id);

    /// <summary>
    /// 异步加载所有设备数据到内存中。
    /// </summary>
    Task LoadAllDevicesAsync();

}