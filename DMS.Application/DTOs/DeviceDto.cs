namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示设备基本信息的DTO。
/// </summary>
public class DeviceDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Protocol { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public int Rack { get; set; }
    public int Slot { get; set; }
    public string OpcUaServerUrl { get; set; }
    public bool IsActive { get; set; }
    public string Status { get; set; } // "在线", "离线", "连接中..."
}