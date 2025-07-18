using System.ComponentModel;

namespace DMS.Core.Enums;

public enum DeviceType
{
    [Description("西门子PLC")] SiemensPLC,
    [Description("三菱PLC")] MelsecPLC
}