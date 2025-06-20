using System.ComponentModel;

namespace PMSWPF.Enums;

public enum SignalType
{
    [Description("启动信号")]
    StartSignal,
    [Description("停止信号")]
    StopSignal,
    [Description("报警信号")]
    AlarmSignal,
    [Description("准备信号")]
    ReadySignal,
    [Description("复位信号")]
    ResetSignal,
    [Description("运行信号")]
    RunSignal,
    [Description("设定频率")]
    SetHZSignal,
    [Description("当前频率")]
    GetHZSignal,
    [Description("当前电流")]
    CurrentASignal,
    [Description("其他信号")]
    OtherASignal
    
}