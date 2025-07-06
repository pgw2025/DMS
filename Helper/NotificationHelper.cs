using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Messaging;
using NLog;
using PMSWPF.Enums;
using PMSWPF.Message;

namespace PMSWPF.Helper;

/// <summary>
/// 通知帮助类，用于显示各种类型的通知消息，并集成日志记录功能。
/// </summary>
public class NotificationHelper
{
    /// <summary>
    /// 获取当前类的 NLog 日志实例。
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 显示一个通用通知消息，并根据通知类型记录日志。
    /// </summary>
    /// <param name="msg">通知消息内容。</param>
    /// <param name="notificationType">通知类型（如信息、错误、成功等），默认为信息。</param>
    /// <param name="isGlobal">是否为全局通知（目前未使用，保留参数）。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void ShowMessage(string msg, NotificationType notificationType = NotificationType.Info,
                                   bool isGlobal = false, [CallerFilePath] string callerFilePath = "",
                                   [CallerLineNumber] int callerLineNumber = 0)
    {
        // 根据通知类型记录日志
        if (notificationType == NotificationType.Error)
        {
            Logger.Error($"{msg} (File: {callerFilePath}, Line: {callerLineNumber})");
        }
        else
        {
            Logger.Info($"{msg} (File: {callerFilePath}, Line: {callerLineNumber})");
        }

        // 通过消息总线发送通知消息，以便UI层可以订阅并显示
        WeakReferenceMessenger.Default.Send<NotificationMessage>(
            new NotificationMessage(msg, notificationType));
    }

    /// <summary>
    /// 显示一个错误通知消息，并记录错误日志。
    /// </summary>
    /// <param name="msg">错误消息内容。</param>
    /// <param name="exception">可选：要记录的异常对象。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void ShowError(string msg, Exception exception = null,
                                 [CallerFilePath] string callerFilePath = "",
                                 [CallerMemberName] string callerMember = "",
                                 [CallerLineNumber] int callerLineNumber = 0)
    {
        // 使用 using 语句确保 MappedDiagnosticsLogicalContext 在作用域结束时被清理。
        // 这对于异步方法尤其重要，因为上下文会随着异步操作的流转而传递。
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerFilePath", callerFilePath))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerLineNumber", callerLineNumber))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerMember", callerMember))
        {
            // 记录错误日志，包含异常信息（如果提供）
            Logger.Error(exception, msg);
            // 通过消息总线发送错误通知
            WeakReferenceMessenger.Default.Send<NotificationMessage>(
                new NotificationMessage(msg, NotificationType.Error));
        }
    }
    
    /// <summary>
    /// 显示一个成功通知消息，并记录信息日志。
    /// </summary>
    /// <param name="msg">成功消息内容。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void ShowSuccess(string msg,
                                   [CallerFilePath] string callerFilePath = "",
                                   [CallerMemberName] string callerMember = "",
                                   [CallerLineNumber] int callerLineNumber = 0)
    {
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerFilePath", callerFilePath))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerLineNumber", callerLineNumber))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerMember", callerMember))
        {
            // 记录信息日志
            Logger.Info(msg);
            // 通过消息总线发送成功通知
            WeakReferenceMessenger.Default.Send<NotificationMessage>(
                new NotificationMessage(msg, NotificationType.Success));
        }
    }

    /// <summary>
    /// 显示一个信息通知消息，并记录信息日志。
    /// </summary>
    /// <param name="msg">信息消息内容。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void ShowInfo(string msg,
                                   [CallerFilePath] string callerFilePath = "",
                                   [CallerMemberName] string callerMember = "",
                                   [CallerLineNumber] int callerLineNumber = 0)
    {
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerFilePath", callerFilePath))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerLineNumber", callerLineNumber))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerMember", callerMember))
        {
            // 记录信息日志
            Logger.Info(msg);
            // 通过消息总线发送信息通知
            WeakReferenceMessenger.Default.Send<NotificationMessage>(
                new NotificationMessage(msg, NotificationType.Info));
        }
    }
}