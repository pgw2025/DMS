
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using NLog;

namespace PMSWPF.Helper;

/// <summary>
/// NLog 日志帮助类，提供简化的日志记录方法，并自动捕获调用者信息。
/// 新增了日志节流功能，以防止在短时间内产生大量重复的日志（日志风暴）。
/// </summary>
public static class NlogHelper
{
    /// <summary>
    /// 获取当前类的 NLog 日志实例。
    /// </summary>
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 内部类，用于存储节流日志的状态信息。
    /// </summary>
    private class ThrottledLogInfo
    {
        /// <summary>
        /// 日志在节流时间窗口内的调用次数。
        /// 使用 int 类型，并通过 Interlocked.Increment 进行原子性递增，确保线程安全。
        /// </summary>
        public int Count;

        /// <summary>
        /// 用于在节流时间结束后执行操作的计时器。
        /// </summary>
        public Timer Timer;

        /// <summary>
        /// 调用日志方法的源文件完整路径。
        /// </summary>
        public string CallerFilePath;

        /// <summary>
        /// 调用日志方法的成员或属性名称。
        /// </summary>
        public string CallerMember;

        /// <summary>
        /// 调用日志方法的行号。
        /// </summary>
        public int CallerLineNumber;

        /// <summary>
        /// 日志级别 (e.g., Info, Error)。
        /// </summary>
        public LogLevel Level;

        /// <summary>
        /// 关联的异常对象（如果有）。
        /// </summary>
        public Exception Exception;
    }

    /// <summary>
    /// 线程安全的字典，用于存储正在被节流的日志。
    /// 键 (string) 是根据日志消息和调用位置生成的唯一标识。
    /// 值 (ThrottledLogInfo) 是该日志的节流状态信息。
    /// </summary>
    private static readonly ConcurrentDictionary<string, ThrottledLogInfo> ThrottledLogs = new ConcurrentDictionary<string, ThrottledLogInfo>();

    /// <summary>
    /// 定义节流的时间窗口（单位：秒）。
    /// </summary>
    private const int ThrottleTimeSeconds = 10;

    /// <summary>
    /// 内部核心日志记录方法，包含了节流逻辑。
    /// </summary>
    /// <param name="msg">日志消息内容。</param>
    /// <param name="level">NLog 的日志级别。</param>
    /// <param name="exception">可选的异常对象。</param>
    /// <param name="throttle">是否启用节流。</param>
    /// <param name="callerFilePath">调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">调用此方法的行号。</param>
    private static void LogInternal(string msg, LogLevel level, Exception exception, bool throttle, string callerFilePath, string callerMember, int callerLineNumber)
    {
        // 如果不启用节流，则直接记录日志并返回。
        if (!throttle)
        {
            LogWithContext(msg, level, exception, callerFilePath, callerMember, callerLineNumber);
            return;
        }

        // 使用消息内容和调用位置生成唯一键，以区分不同的日志来源。
        var key = $"{callerFilePath}:{callerLineNumber}:{msg}";

        // 使用 AddOrUpdate 实现原子操作，确保线程安全。
        // 它会尝试添加一个新的节流日志条目，如果键已存在，则更新现有条目。
        ThrottledLogs.AddOrUpdate(
            key,
            // --- 添加逻辑 (addValueFactory)：当日志第一次被节流时执行 ---
            _ =>
            {
                // 1. 首次出现，立即记录一次原始日志。
                LogWithContext(msg, level, exception, callerFilePath, callerMember, callerLineNumber);

                // 2. 创建一个新的节流信息对象。
                var newThrottledLog = new ThrottledLogInfo
                {
                    Count = 1,
                    CallerFilePath = callerFilePath,
                    CallerMember = callerMember,
                    CallerLineNumber = callerLineNumber,
                    Level = level,
                    Exception = exception
                };

                // 3. 创建并启动一个一次性计时器。
                newThrottledLog.Timer = new Timer(s =>
                {
                    // --- 计时器回调：在指定时间（例如30秒）后触发 ---
                    // 尝试从字典中移除当前日志条目。
                    if (ThrottledLogs.TryRemove(key, out var finishedLog))
                    {
                        // 释放计时器资源。
                        finishedLog.Timer.Dispose();
                        // 如果在节流期间有后续调用（Count > 1），则记录一条摘要日志。
                        if (finishedLog.Count > 1)
                        {
                            var summaryMsg = $"日志 '{msg}' 在过去 {ThrottleTimeSeconds} 秒内被调用 {finishedLog.Count} 次。";
                            LogWithContext(summaryMsg, finishedLog.Level, finishedLog.Exception, finishedLog.CallerFilePath, finishedLog.CallerMember, finishedLog.CallerLineNumber);
                        }
                    }
                }, null, ThrottleTimeSeconds * 1000, Timeout.Infinite); // 设置30秒后触发，且不重复。

                return newThrottledLog;
            },
            // --- 更新逻辑 (updateValueFactory)：当日志在节流窗口内再次被调用时执行 ---
            (_, existingLog) =>
            {
                // 只需将调用次数加一。使用 Interlocked.Increment 保证原子操作，避免多线程下的竞态条件。
                Interlocked.Increment(ref existingLog.Count);
                return existingLog;
            });
    }

    /// <summary>
    /// 将日志信息包装在 NLog 的 MappedDiagnosticsLogicalContext 中进行记录。
    /// 这允许在 nlog.config 配置文件中使用 ${mdlc:item=...} 来获取调用者信息。
    /// </summary>
    private static void LogWithContext(string msg, LogLevel level, Exception exception, string callerFilePath, string callerMember, int callerLineNumber)
    {
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerFilePath", callerFilePath))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerLineNumber", callerLineNumber))
        using (MappedDiagnosticsLogicalContext.SetScoped("CallerMember", callerMember))
        {
            if (exception != null)
            {
                Logger.Log(level, exception, msg);
            }
            else
            {
                Logger.Log(level, msg);
            }
        }
    }

    /// <summary>
    /// 记录一个错误级别的日志，支持节流。
    /// </summary>
    /// <param name="msg">日志消息内容。</param>
    /// <param name="exception">可选：要记录的异常对象。</param>
    /// <param name="throttle">是否启用日志节流。如果为 true，则在30秒内对来自同一位置的相同日志消息进行节流处理。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void Error(string msg, Exception exception = null, bool throttle = false,
                             [CallerFilePath] string callerFilePath = "",
                             [CallerMemberName] string callerMember = "",
                             [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(msg, LogLevel.Error, exception, throttle, callerFilePath, callerMember, callerLineNumber);
    }

    /// <summary>
    /// 记录一个信息级别的日志，支持节流。
    /// </summary>
    /// <param name="msg">日志消息内容。</param>
    /// <param name="throttle">是否启用日志节流。如果为 true，则在30秒内对来自同一位置的相同日志消息进行节流处理。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void Info(string msg, bool throttle = false,
                            [CallerFilePath] string callerFilePath = "",
                            [CallerMemberName] string callerMember = "",
                            [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(msg, LogLevel.Info, null, throttle, callerFilePath, callerMember, callerLineNumber);
    }

    /// <summary>
    /// 记录一个警告级别的日志，支持节流。
    /// </summary>
    /// <param name="msg">日志消息内容。</param>
    /// <param name="throttle">是否启用日志节流。如果为 true，则在30秒内对来自同一位置的相同日志消息进行节流处理。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void Warn(string msg, bool throttle = false,
                            [CallerFilePath] string callerFilePath = "",
                            [CallerMemberName] string callerMember = "",
                            [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(msg, LogLevel.Warn, null, throttle, callerFilePath, callerMember, callerLineNumber);
    }

    /// <summary>
    /// 记录一个跟踪级别的日志，支持节流。
    /// </summary>
    /// <param name="msg">日志消息内容。</param>
    /// <param name="throttle">是否启用日志节流。如果为 true，则在30秒内对来自同一位置的相同日志消息进行节流处理。</param>
    /// <param name="callerFilePath">自动捕获：调用此方法的源文件完整路径。</param>
    /// <param name="callerMember">自动捕获：调用此方法的成员或属性名称。</param>
    /// <param name="callerLineNumber">自动捕获：调用此方法的行号。</param>
    public static void Trace(string msg, bool throttle = false,
                             [CallerFilePath] string callerFilePath = "",
                             [CallerMemberName] string callerMember = "",
                             [CallerLineNumber] int callerLineNumber = 0)
    {
        LogInternal(msg, LogLevel.Trace, null, throttle, callerFilePath, callerMember, callerLineNumber);
    }
}
