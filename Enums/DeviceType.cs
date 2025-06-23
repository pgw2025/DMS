using System.ComponentModel;

namespace PMSWPF.Enums;

public enum DeviceType
{
    [Description("西门子PLC")] SiemensPLC,
    [Description("三菱PLC")] MelsecPLC
}