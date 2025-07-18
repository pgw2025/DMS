using System.ComponentModel;

namespace DMS.Enums;

public enum DeviceType
{
    [Description("西门子PLC")] SiemensPLC,
    [Description("三菱PLC")] MelsecPLC
}