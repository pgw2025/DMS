using System.Collections.ObjectModel;
using DMS.Application.DTOs;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.Interfaces;

/// <summary>
/// 设备数据服务接口。
/// </summary>
public interface IDeviceDataService
{
    /// <summary>
    /// 设备列表。
    /// </summary>
    // ObservableCollection<DeviceItem> Devices { get; set; }

    /// <summary>
    /// 加载所有设备数据。
    /// </summary>
    void LoadAllDevices();

    /// <summary>
    /// 添加设备。
    /// </summary>
    Task<CreateDeviceWithDetailsDto> AddDevice(CreateDeviceWithDetailsDto dto);

    /// <summary>
    /// 删除设备。
    /// </summary>
    Task<bool> DeleteDevice(DeviceItem device);

    /// <summary>
    /// 更新设备。
    /// </summary>
    Task<bool> UpdateDevice(DeviceItem device);
    Task<CreateDeviceWithDetailsDto?> AddDevice(CreateDeviceWithDetailsDto dto);
}