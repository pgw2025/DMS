using DMS.Core.Models;

namespace DMS.Core.Interfaces;

/// <summary>
/// 继承自IBaseRepository，提供设备相关的特定数据查询功能。
/// </summary>
public interface IDeviceRepository : IBaseRepository<Device>
{
    /// <summary>
    /// 异步获取所有激活的设备，并级联加载其下的变量表和变量。
    /// 这是后台轮询服务需要的主要数据。
    /// </summary>
    /// <returns>包含完整层级结构的激活设备列表。</returns>
    Task<List<Device>> GetActiveDevicesWithDetailsAsync(ProtocolType protocol);

    /// <summary>
    /// 异步根据设备ID获取设备及其所有详细信息（变量表、变量、MQTT别名等）。
    /// </summary>
    /// <param name="deviceId">设备ID。</param>
    /// <returns>包含详细信息的设备对象。</returns>
    Task<Device> GetDeviceWithDetailsAsync(int deviceId);
}