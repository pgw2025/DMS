using System.ComponentModel;

namespace DMS.Enums;

public enum ProtocolType
{
    [Description("S7协议")] S7,
    [Description("OpcUA协议")] OpcUA,
    [Description("ModbusRtu协议")] ModbusRtu,
    [Description("ModbusTcp协议")] ModbusTcp
}