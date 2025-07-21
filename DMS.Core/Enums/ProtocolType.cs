/// <summary>
/// 定义了设备支持的通信协议类型。
/// </summary>
public enum ProtocolType
{
    /// <summary>
    /// Siemens S7 通信协议。
    /// </summary>
    S7,

    /// <summary>
    /// OPC UA (Unified Architecture) 协议。
    /// </summary>
    OpcUa,

    /// <summary>
    /// Modbus TCP 协议。
    /// </summary>
    ModbusTcp
}