using System.ComponentModel;

namespace DMS.Core.Enums;

public enum SignalType
{
    [Description("启动信号")] StartSignal,
    [Description("停止信号")] StopSignal,
    [Description("报警信号")] AlarmSignal,
    [Description("准备信号")] ReadySignal,
    [Description("复位信号")] ResetSignal,
    [Description("运行信号")] RunSignal,
    [Description("设定频率")] SetHZSignal,
    [Description("当前频率")] GetHZSignal,
    [Description("当前电流")] CurrentASignal,
    [Description("其他信号")] OtherASignal
}

/// <summary>
/// 定义了C#中常用的数据类型。
/// </summary>
public enum CSharpDataType
{
    [Description("布尔型")] Bool,
    [Description("字节型")] Byte,
    [Description("短整型")] Short,
    [Description("无符号短整型")] UShort,
    [Description("整型")] Int,
    [Description("无符号整型")] UInt,
    [Description("长整型")] Long,
    [Description("无符号长整型")] ULong,
    [Description("浮点型")] Float,
    [Description("双精度浮点型")] Double,
    [Description("字符串型")] String,
    [Description("日期时间型")] DateTime,
    [Description("时间跨度型")] TimeSpan,
    [Description("对象型")] Object,
    [Description("未知类型")] Unknown
}