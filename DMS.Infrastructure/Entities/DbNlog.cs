using SqlSugar;
using System;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 数据库实体：对应数据库中的 Logs 表，用于存储应用程序日志。
/// </summary>
public class DbNlog
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>
    /// 日志记录的时间戳。
    /// </summary>
    public DateTime Logged { get; set; }

    /// <summary>
    /// 日志级别 (e.g., "Info", "Warn", "Error", "Debug")。
    /// </summary>
    public string Level { get; set; }

    /// <summary>
    /// 日志消息主体。
    /// </summary>
    [SugarColumn(Length = -1)] // 映射为NVARCHAR(MAX)或类似类型
    public string Message { get; set; }

    /// <summary>
    /// 异常信息，包括堆栈跟踪。如果无异常则为null。
    /// </summary>
    [SugarColumn(IsNullable = true, Length = -1)]
    public string Exception { get; set; }

    /// <summary>
    /// 记录日志的调用点信息 (文件路径:行号)。
    /// </summary>
    public string CallSite { get; set; }

    /// <summary>
    /// 记录日志的方法名。
    /// </summary>
    public string MethodName { get; set; }

    /// <summary>
    /// (用于聚合) 此条日志在指定时间窗口内被触发的总次数。默认为1。
    /// </summary>
    public int AggregatedCount { get; set; } = 1;
}