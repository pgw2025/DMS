using SqlSugar;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 设备实体类，对应数据库中的 Devices 表。
/// </summary>
[SugarTable("Devices")]
public class DbDevice
{
    /// <summary>
    /// 设备ID，主键且自增。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 设备名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 设备通信协议类型，对应 ProtocolType 枚举。
    /// </summary>
    public ProtocolType Protocol { get; set; }

    /// <summary>
    /// 设备IP地址。
    /// </summary>
    public string IpAddress { get; set; }

    /// <summary>
    /// 设备端口号。
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 设备机架号 (针对PLC等设备)。
    /// </summary>
    public int Rack { get; set; }

    /// <summary>
    /// 设备槽号 (针对PLC等设备)。
    /// </summary>
    public int Slot { get; set; }

    /// <summary>
    /// OPC UA服务器的URL地址。
    /// </summary>
    public string OpcUaServerUrl { get; set; }

    /// <summary>
    /// 设备是否激活/启用。
    /// </summary>
    public bool IsActive { get; set; }
}