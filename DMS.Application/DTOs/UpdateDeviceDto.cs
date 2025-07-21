using DMS.Core.Enums;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于更新设备时传输数据的DTO。
/// </summary>
public class UpdateDeviceDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ProtocolType Protocol { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public int Rack { get; set; }
    public int Slot { get; set; }
    public string OpcUaServerUrl { get; set; }
    public bool IsActive { get; set; }
}