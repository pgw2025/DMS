using DMS.Core.Enums;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于创建新设备时传输数据的DTO。
/// </summary>
public class CreateDeviceDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public ProtocolType Protocol { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public int Rack { get; set; }
    public int Slot { get; set; }
    public string CpuType { get; set; }
    
    public DeviceType DeviceType { get; set; }
    public string OpcUaServerUrl { get; set; }
    public bool IsActive { get; set; }
}