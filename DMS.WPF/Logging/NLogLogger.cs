using System;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using NLogLevel = NLog.LogLevel;

namespace DMS.WPF.Logging;

/// <summary>
/// NLog日志服务实现，直接使用NLog记录日志，并实现Microsoft.Extensions.Logging.ILogger接口
/// </summary>
public class NLogLogger : ILogger
{
    /// <summary>
    /// 获取当前类的 NLog 日志实例。
    /// </summary>
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    /// <summary>
    /// 日志记录器名称
    /// </summary>
    private readonly string _name;

    public NLogLogger()
    {
        _name = nameof(NLogLogger);
    }

    public NLogLogger(string name)
    {
        _name = name;
    }

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
    }

    /// <summary>
    /// 线程安全的字典，用于存储正在被节流的日志。
    /// 键 (string) 是根据日志消息生成的唯一标识。
    /// 值 (ThrottledLogInfo) 是该日志的节流状态信息。
    /// </summary>
    private static readonly ConcurrentDictionary<string, ThrottledLogInfo> ThrottledLogs = new ConcurrentDictionary<string, ThrottledLogInfo>();

    /// <summary>
    /// 定义节流的时间窗口（单位：秒）。
    /// </summary>
    private const int ThrottleTimeSeconds = 30;

    /// <summary>
    /// 内部核心日志记录方法，包含了节流逻辑。
    /// </summary>
    /// <param name="msg">日志消息内容。</param>
    /// <param name="level">NLog 的日志级别。</param>
    /// <param name="exception">可选的异常对象。</param>
    /// <param name="throttle">是否启用节流。</param>
    private void LogInternal(string msg, NLogLevel level, Exception exception, bool throttle)
    {
        // 如果不启用节流，则直接记录日志并返回。
        if (!throttle)
        {
            Logger.Log(level, exception, msg);
            return;
        }

        // 使用消息内容生成唯一键，以区分不同的日志来源。
        var key = msg;

        // 使用 AddOrUpdate 实现原子操作，确保线程安全。
        // 它会尝试添加一个新的节流日志条目，如果键已存在，则更新现有条目。
        ThrottledLogs.AddOrUpdate(
            key,
            // --- 添加逻辑 (addValueFactory)：当日志第一次被节流时执行 ---
            _ =>
            {
                // 1. 首次出现，立即记录一次原始日志。
                Logger.Log(level, exception, msg);

                // 2. 创建一个新的节流信息对象。
                var newThrottledLog = new ThrottledLogInfo
                {
                    Count = 1
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
                            Logger.Log(level, summaryMsg);
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

    public IDisposable BeginScope<TState>(TState state)
    {
        return null; // NLog不直接支持作用域
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return Logger.IsEnabled(ToNLogLevel(logLevel));
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        var nlogLevel = ToNLogLevel(logLevel);

        switch (logLevel)
        {
            case LogLevel.Trace:
                LogInternal(message, nlogLevel, exception, false);
                break;
            case LogLevel.Debug:
                LogInternal(message, nlogLevel, exception, false);
                break;
            case LogLevel.Information:
                LogInternal(message, nlogLevel, exception, false);
                break;
            case LogLevel.Warning:
                LogInternal(message, nlogLevel, exception, false);
                break;
            case LogLevel.Error:
                LogInternal(message, nlogLevel, exception, false);
                break;
            case LogLevel.Critical:
                LogInternal($"[Critical] {message}", nlogLevel, exception, false);
                break;
        }
    }

    private NLogLevel ToNLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => NLogLevel.Trace,
            LogLevel.Debug => NLogLevel.Debug,
            LogLevel.Information => NLogLevel.Info,
            LogLevel.Warning => NLogLevel.Warn,
            LogLevel.Error => NLogLevel.Error,
            LogLevel.Critical => NLogLevel.Fatal,
            _ => NLogLevel.Debug
        };
    }
    
    // 保持原有的自定义方法以提供向后兼容性
    public void LogError(string message, Exception exception = null)
    {
        LogInternal(message, NLogLevel.Error, exception, true);
    }

    public void LogInfo(string message)
    {
        LogInternal(message, NLogLevel.Info, null, true);
    }

    public void LogWarning(string message)
    {
        LogInternal(message, NLogLevel.Warn, null, true);
    }

    public void LogDebug(string message)
    {
        LogInternal(message, NLogLevel.Debug, null, true);
    }

    public void LogTrace(string message)
    {
        LogInternal(message, NLogLevel.Trace, null, true);
    }
}