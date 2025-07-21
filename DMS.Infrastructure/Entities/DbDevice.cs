using DMS.Core.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 设备实体类，对应数据库中的 Devices 表。
/// </summary>
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
    /// 设备描述。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string Description { get; set; }

    /// <summary>
    /// 设备通信协议类型，对应 ProtocolType 枚举。
    /// </summary>
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
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
    [SugarColumn(IsNullable = true)]
    public int Rack { get; set; }

    /// <summary>
    /// 设备槽号 (针对PLC等设备)。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public int Slot { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public string CpuType { get; set; }
    /// <summary>
    /// 设备槽号 (针对PLC等设备)。
    /// </summary>
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public DeviceType DeviceType { get; set; }

    /// <summary>
    /// OPC UA服务器的URL地址。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string OpcUaServerUrl { get; set; }

    /// <summary>
    /// 设备是否激活/启用。
    /// </summary>
    public bool IsActive { get; set; }
}