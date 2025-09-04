using System;
using System.Runtime.CompilerServices;
using DMS.Core.Enums;

namespace DMS.WPF.Interfaces;

public interface INotificationService
{
    /// <summary>
    /// 显示一个通用通知消息，并根据通知类型记录日志。支持节流。
    /// </summary>
    /// <param name="msg">通知消息内容。</param>
    /// <param name="notificationType">通知类型（如信息、错误、成功等）。</param>
    /// <param name="throttle">是否启用通知节流。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    void ShowMessage(string msg, NotificationType notificationType = NotificationType.Info, bool throttle = true,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0);

    /// <summary>
    /// 显示一个错误通知消息，并记录错误日志。支持节流。
    /// </summary>
    /// <param name="msg">错误消息内容。</param>
    /// <param name="exception">可选：要记录的异常对象。</param>
    /// <param name="throttle">是否启用通知和日志节流。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    void ShowError(string msg, Exception exception = null, bool throttle = true,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string callerMember = "",
        [CallerLineNumber] int callerLineNumber = 0);

    /// <summary>
    /// 显示一个成功通知消息，并记录信息日志。支持节流。
    /// </summary>
    /// <param name="msg">成功消息内容。</param>
    /// <param name="throttle">是否启用通知和日志节流。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    void ShowSuccess(string msg, bool throttle = true,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string callerMember = "",
        [CallerLineNumber] int callerLineNumber = 0);

    /// <summary>
    /// 显示一个信息通知消息，并记录信息日志。支持节流。
    /// </summary>
    /// <param name="msg">信息消息内容。</param>
    /// <param name="throttle">是否启用通知和日志节流。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    void ShowInfo(string msg, bool throttle = true,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string callerMember = "",
        [CallerLineNumber] int callerLineNumber = 0);

    /// <summary>
    /// 显示一个警告通知消息，并记录警告日志。支持节流。
    /// </summary>
    /// <param name="msg">警告消息内容。</param>
    /// <param name="throttle">是否启用通知和日志节流。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    void ShowWarn(string msg, bool throttle = true,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string callerMember = "",
        [CallerLineNumber] int callerLineNumber = 0);
}