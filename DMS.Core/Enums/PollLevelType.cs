using System.ComponentModel;

namespace DMS.Core.Enums
{
    public enum PollLevelType
    {
        [Description("10毫秒")]
        TenMilliseconds = 10,
        [Description("100毫秒")]
        HundredMilliseconds = 100,
        [Description("500毫秒")]
        FiveHundredMilliseconds = 500,
        [Description("1秒钟")]
        OneSecond = 1000,
        [Description("5秒钟")]
        FiveSeconds = 5000,
        [Description("10秒钟")]
        TenSeconds = 10000,
        [Description("20秒钟")]
        TwentySeconds = 20000,
        [Description("30秒钟")]
        ThirtySeconds = 30000,
        [Description("1分钟")]
        OneMinute = 60000,
        [Description("3分钟")]
        ThreeMinutes = 180000,
        [Description("5分钟")]
        FiveMinutes = 300000,
        [Description("10分钟")]
        TenMinutes = 600000,
        [Description("30分钟")]
        ThirtyMinutes = 1800000,
        [Description("1小时")]
        OneHour = 3600000
    }
}