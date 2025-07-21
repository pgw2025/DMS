using DMS.Core.Enums;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于创建新设备时传输数据的DTO。
/// </summary>
public class CreateDeviceDto
{
    public string Name { get; set; }
    public ProtocolType Protocol { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
}