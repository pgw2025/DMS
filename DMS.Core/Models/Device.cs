using DMS.Core.Enums;
using System.Collections.Generic;

namespace DMS.Core.Models;

/// <summary>
/// 代表一个可管理的物理或逻辑设备。
/// </summary>
public class Device
{
    /// <summary>
    /// 唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 设备名称，用于UI显示和识别。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 设备的描述信息。
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 设备使用的通信协议。
    /// </summary>
    public ProtocolType Protocol { get; set; }

    /// <summary>
    /// 设备的IP地址。
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// 设备的通信端口号。
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// S7 PLC的机架号。
    /// </summary>
    public short Rack { get; set; }

    /// <summary>
    /// S7 PLC的槽号。
    /// </summary>
    public short Slot { get; set; }

    /// <summary>
    /// OPC UA 服务器地址 (仅当 Protocol 为 OpcUa 时有效)。
    /// </summary>
    public string OpcUaServerUrl { get; set; }

    /// <summary>
    /// 指示此设备是否处于激活状态。只有激活的设备才会被轮询。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 此设备包含的变量表集合。
    /// </summary>
    public List<VariableTable> VariableTables { get; set; } = new();

    public CpuType CpuType { get; set; }
    public DeviceType DeviceType { get; set; }
    public bool IsRunning { get; set; }
}