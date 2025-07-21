using System.ComponentModel;

namespace DMS.Core.Enums;

public enum DeviceType
{
    [Description("西门子PLC")] SiemensPLC,
    [Description("OpcUa设备")] OpcUa,
    [Description("Modbus TCP设备")] ModbusTCP,
    [Description("三菱PLC")] MelsecPLC
}