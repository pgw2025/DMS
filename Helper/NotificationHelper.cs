
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using CommunityToolkit.Mvvm.Messaging;
using PMSWPF.Enums;
using PMSWPF.Message;

namespace PMSWPF.Helper;

/// <summary>
/// 通知帮助类，用于显示各种类型的通知消息，并集成日志记录功能。
/// 新增了通知节流功能，以防止在短时间内向用户发送大量重复的通知。
/// </summary>
public static class NotificationHelper
{
    /// <summary>
    /// 内部类，用于存储节流通知的状态信息。
    /// </summary>
    private class ThrottledNotificationInfo
    {
        public int Count;
        public Timer Timer;
        public NotificationType NotificationType;
    }

    private static readonly ConcurrentDictionary<string, ThrottledNotificationInfo> ThrottledNotifications = new ConcurrentDictionary<string, ThrottledNotificationInfo>();
    private const int ThrottleTimeSeconds = 30;

    /// <summary>
    /// 内部核心通知发送方法，包含了节流逻辑。
    /// </summary>
    private static void SendNotificationInternal(string msg, NotificationType notificationType, bool throttle, Exception exception, string callerFilePath, string callerMember, int callerLineNumber)
    {
        // 根据通知类型决定日志级别，并使用 NlogHelper 记录日志（利用其自身的节流逻辑）
        if (notificationType == NotificationType.Error)
        {
            NlogHelper.Error(msg, exception, throttle, callerFilePath, callerMember, callerLineNumber);
        }
        else
        {
            NlogHelper.Info(msg, throttle, callerFilePath, callerMember, callerLineNumber);
        }

        // 如果不启用通知节流，则直接发送通知并返回。
        if (!throttle)
        {
            WeakReferenceMessenger.Default.Send(new NotificationMessage(msg, notificationType));
            return;
        }

        var key = $"{callerFilePath}:{callerLineNumber}:{msg}";

        ThrottledNotifications.AddOrUpdate(
            key,
            // --- 添加逻辑：当通知第一次被节流时执行 ---
            _ =>
            {
                // 1. 首次出现，立即发送一次通知。
                WeakReferenceMessenger.Default.Send(new NotificationMessage(msg, notificationType));

                // 2. 创建新的节流信息对象。
                var newThrottledNotification = new ThrottledNotificationInfo
                {
                    Count = 1,
                    NotificationType = notificationType
                };

                // 3. 创建并启动计时器。
                newThrottledNotification.Timer = new Timer(s =>
                {
                    if (ThrottledNotifications.TryRemove(key, out var finishedNotification))
                    {
                        finishedNotification.Timer.Dispose();
                        if (finishedNotification.Count > 1)
                        {
                            var summaryMsg = $"消息 '{msg}' 在过去 {ThrottleTimeSeconds} 秒内出现了 {finishedNotification.Count} 次。";
                            WeakReferenceMessenger.Default.Send(new NotificationMessage(summaryMsg, finishedNotification.NotificationType));
                        }
                    }
                }, null, ThrottleTimeSeconds * 1000, Timeout.Infinite);

                return newThrottledNotification;
            },
            // --- 更新逻辑：当通知在节流窗口内再次出现时执行 ---
            (_, existingNotification) =>
            {
                Interlocked.Increment(ref existingNotification.Count);
                return existingNotification;
            });
    }

    /// <summary>
    /// 显示一个通用通知消息，并根据通知类型记录日志。支持节流。
    /// </summary>
    /// <param name="msg">通知消息内容。</param>
    /// <param name="notificationType">通知类型（如信息、错误、成功等）。</param>
    /// <param name="throttle">是否启用通知节流。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void ShowMessage(string msg, NotificationType notificationType = NotificationType.Info, bool throttle = true,
                                   [CallerFilePath] string callerFilePath = "",
                                   [CallerLineNumber] int callerLineNumber = 0)
    {
        SendNotificationInternal(msg, notificationType, throttle, null, callerFilePath, "", callerLineNumber);
    }

    /// <summary>
    /// 显示一个错误通知消息，并记录错误日志。支持节流。
    /// </summary>
    /// <param name="msg">错误消息内容。</param>
    /// <param name="exception">可选：要记录的异常对象。</param>
    /// <param name="throttle">是否启用通知和日志节流。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void ShowError(string msg, Exception exception = null, bool throttle = true,
                                 [CallerFilePath] string callerFilePath = "",
                                 [CallerMemberName] string callerMember = "",
                                 [CallerLineNumber] int callerLineNumber = 0)
    {
        SendNotificationInternal(msg, NotificationType.Error, throttle, exception, callerFilePath, callerMember, callerLineNumber);
    }

    /// <summary>
    /// 显示一个成功通知消息，并记录信息日志。支持节流。
    /// </summary>
    /// <param name="msg">成功消息内容。</param>
    /// <param name="throttle">是否启用通知和日志节流。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void ShowSuccess(string msg, bool throttle = true,
                                   [CallerFilePath] string callerFilePath = "",
                                   [CallerMemberName] string callerMember = "",
                                   [CallerLineNumber] int callerLineNumber = 0)
    {
        SendNotificationInternal(msg, NotificationType.Success, throttle, null, callerFilePath, callerMember, callerLineNumber);
    }

    /// <summary>
    /// 显示一个信息通知消息，并记录信息日志。支持节流。
    /// </summary>
    /// <param name="msg">信息消息内容。</param>
    /// <param name="throttle">是否启用通知和日志节流。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void ShowInfo(string msg, bool throttle = true,
                                [CallerFilePath] string callerFilePath = "",
                                [CallerMemberName] string callerMember = "",
                                [CallerLineNumber] int callerLineNumber = 0)
    {
        SendNotificationInternal(msg, NotificationType.Info, throttle, null, callerFilePath, callerMember, callerLineNumber);
    }
}
