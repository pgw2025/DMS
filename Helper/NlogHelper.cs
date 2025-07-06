using System.Runtime.CompilerServices;
using NLog;

namespace PMSWPF.Helper;

/// <summary>
/// NLog 日志帮助类，提供简化的日志记录方法，并自动捕获调用者信息。
/// </summary>
public static class NlogHelper
{
    /// <summary>
    /// 获取当前类的 NLog 日志实例。
    /// </summary>
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 记录一个错误级别的日志。
    /// </summary>
    /// <param name="msg">日志消息内容。</param>
    /// <param name="exception">可选：要记录的异常对象。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void Error(string msg, Exception exception = null,
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
            Logger.Error(exception, msg);
        }
    }

    /// <summary>
    /// 记录一个信息级别的日志。
    /// </summary>
    /// <param name="msg">日志消息内容。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void Info(string msg,
                            [CallerFilePath] string callerFilePath = "",
                            [CallerMemberName] string callerMember = "",
                            [CallerLineNumber] int callerLineNumber = 0)
    {
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerFilePath", callerFilePath))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerLineNumber", callerLineNumber))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerMember", callerMember))
        {
            Logger.Info(msg);
        }
    }

    /// <summary>
    /// 记录一个警告级别的日志。
    /// </summary>
    /// <param name="msg">日志消息内容。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void Warn(string msg,
                            [CallerFilePath] string callerFilePath = "",
                            [CallerMemberName] string callerMember = "",
                            [CallerLineNumber] int callerLineNumber = 0)
    {
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerFilePath", callerFilePath))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerLineNumber", callerLineNumber))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerMember", callerMember))
        {
            Logger.Warn(msg);
        }
    }

    /// <summary>
    /// 记录一个跟踪级别的日志。
    /// </summary>
    /// <param name="msg">日志消息内容。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void Trace(string msg,
                             [CallerFilePath] string callerFilePath = "",
                             [CallerMemberName] string callerMember = "",
                             [CallerLineNumber] int callerLineNumber = 0)
    {
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerFilePath", callerFilePath))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerLineNumber", callerLineNumber))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerMember", callerMember))
        {
            Logger.Trace(msg);
        }
    }
}