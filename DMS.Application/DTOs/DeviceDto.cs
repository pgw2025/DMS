using DMS.Core.Enums;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示设备基本信息的DTO。
/// </summary>
public class DeviceDto
{
    /// <summary>
    /// 设备唯一标识符
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 设备名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 设备描述信息
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 通信协议类型
    /// </summary>
    public ProtocolType Protocol { get; set; }
    
    /// <summary>
    /// 设备IP地址
    /// </summary>
    public string IpAddress { get; set; }
    
    /// <summary>
    /// 设备端口号
    /// </summary>
    public int Port { get; set; }
    
    /// <summary>
    /// PLC机架号（用于PLC连接）
    /// </summary>
    public int Rack { get; set; }
    
    /// <summary>
    /// PLC插槽号（用于PLC连接）
    /// </summary>
    public int Slot { get; set; }
    
    /// <summary>
    /// CPU类型
    /// </summary>
    public CpuType CpuType { get; set; }
    
    /// <summary>
    /// 设备类型
    /// </summary>
    public DeviceType DeviceType { get; set; }
    
    /// <summary>
    /// OPC UA服务器URL
    /// </summary>
    public string OpcUaServerUrl { get; set; }
    
    /// <summary>
    /// 设备是否处于激活状态
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// 设备是否正在运行
    /// </summary>
    public bool IsRunning { get; set; }
    
    /// <summary>
    /// 设备当前状态（"在线", "离线", "连接中..."）
    /// </summary>
    public string Status { get; set; } // "在线", "离线", "连接中..."
    
    /// <summary>
    /// 设备关联的变量表集合
    /// </summary>
    public List<VariableTableDto> VariableTables { get; set; }
}