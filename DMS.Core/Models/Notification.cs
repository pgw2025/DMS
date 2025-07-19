using DMS.Core.Enums;

namespace DMS.Core.Models;

/// <summary>
/// 表示通知信息。
/// </summary>
public class Notification
{
    /// <summary>
    /// 通知是否为全局通知。
    /// </summary>
    public bool IsGlobal { get; set; }

    /// <summary>
    /// 通知消息内容。
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 通知类型。
    /// </summary>
    public NotificationType Type { get; set; }
}