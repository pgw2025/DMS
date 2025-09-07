namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示NLog日志条目的DTO。
/// </summary>
public class NlogDto
{
    /// <summary>
    /// 日志ID。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 日志记录时间。
    /// </summary>
    public DateTime LogTime { get; set; }

    /// <summary>
    /// 日志级别 (如 INFO, ERROR 等)。
    /// </summary>
    public string Level { get; set; }

    /// <summary>
    /// 线程ID。
    /// </summary>
    public int ThreadId { get; set; }

    /// <summary>
    /// 线程名称。
    /// </summary>
    public string ThreadName { get; set; }

    /// <summary>
    /// 调用点（通常是记录日志的方法名）。
    /// </summary>
    public string Callsite { get; set; }

    /// <summary>
    /// 调用点所在的行号。
    /// </summary>
    public int CallsiteLineNumber { get; set; }

    /// <summary>
    /// 日志消息内容。
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 记录日志的记录器名称。
    /// </summary>
    public string Logger { get; set; }

    /// <summary>
    /// 异常信息（如果有的话）。
    /// </summary>
    public string Exception { get; set; }

    /// <summary>
    /// 调用方文件路径。
    /// </summary>
    public string CallerFilePath { get; set; }

    /// <summary>
    /// 调用方行号。
    /// </summary>
    public int CallerLineNumber { get; set; }

    /// <summary>
    /// 调用方成员（方法名）。
    /// </summary>
    public string CallerMember { get; set; }
}