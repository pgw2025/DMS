using System.ComponentModel;

namespace DMS.Core.Enums;

public enum CpuType
{
    [Description("S7-1200")]
    S71200,
    [Description("S7-1500")]
    S71500,
    [Description("S7-300")]
    S7300,
    [Description("S7-400")]
    S7400
}
