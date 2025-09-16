using DMS.Application.DTOs;
using DMS.Application.Events;

namespace DMS.Application.Interfaces.Management;

public interface IDeviceManagementService
{
    /// <summary>
    /// 当设备数据发生变化时触发
    /// </summary>
    event EventHandler<DeviceChangedEventArgs> OnDeviceChanged;

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
    Task<CreateDeviceWithDetailsDto> CreateDeviceWithDetailsAsync(CreateDeviceWithDetailsDto dto);

    /// <summary>
    /// 异步更新一个已存在的设备。
    /// </summary>
    Task<int> UpdateDeviceAsync(DeviceDto deviceDto);

    /// <summary>
    /// 异步删除一个设备。
    /// </summary>
    Task<bool> DeleteDeviceByIdAsync(int deviceId);

    /// <summary>
    /// 异步切换设备的激活状态。
    /// </summary>
    Task ToggleDeviceActiveStateAsync(int id);

    /// <summary>
    /// 在内存中添加设备
    /// </summary>
    void AddDeviceToMemory(DeviceDto deviceDto);

    /// <summary>
    /// 在内存中更新设备
    /// </summary>
    void UpdateDeviceInMemory(DeviceDto deviceDto);

    /// <summary>
    /// 在内存中删除设备
    /// </summary>
    void RemoveDeviceFromMemory(int deviceId);

  
}