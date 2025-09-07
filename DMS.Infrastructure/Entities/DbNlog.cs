using SqlSugar;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// NLog日志实体类，对应数据库中的 nlog 表。
/// </summary>
[SugarTable("nlog")]
public class DbNlog
{
    /// <summary>
    /// 日志ID，主键且自增。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "ID")]
    public int Id { get; set; }

    /// <summary>
    /// 日志记录时间。
    /// </summary>
    [SugarColumn(ColumnName = "LogTime")]
    public DateTime LogTime { get; set; }

    /// <summary>
    /// 日志级别 (如 INFO, ERROR 等)。
    /// </summary>
    [SugarColumn(ColumnName = "Level")]
    public string Level { get; set; }

    /// <summary>
    /// 线程ID。
    /// </summary>
    [SugarColumn(ColumnName = "ThreadID")]
    public int ThreadId { get; set; }

    /// <summary>
    /// 线程名称。
    /// </summary>
    [SugarColumn(ColumnName = "ThreadName", IsNullable = true)]
    public string ThreadName { get; set; }

    /// <summary>
    /// 调用点（通常是记录日志的方法名）。
    /// </summary>
    [SugarColumn(ColumnName = "Callsite", IsNullable = true)]
    public string Callsite { get; set; }

    /// <summary>
    /// 调用点所在的行号。
    /// </summary>
    [SugarColumn(ColumnName = "CallsiteLineNumber")]
    public int CallsiteLineNumber { get; set; }

    /// <summary>
    /// 日志消息内容。
    /// </summary>
    [SugarColumn(ColumnName = "Message")]
    public string Message { get; set; }

    /// <summary>
    /// 记录日志的记录器名称。
    /// </summary>
    [SugarColumn(ColumnName = "Logger")]
    public string Logger { get; set; }

    /// <summary>
    /// 异常信息（如果有的话）。
    /// </summary>
    [SugarColumn(ColumnName = "Exception", IsNullable = true)]
    public string Exception { get; set; }

    /// <summary>
    /// 调用方文件路径。
    /// </summary>
    [SugarColumn(ColumnName = "CallerFilePath", IsNullable = true)]
    public string CallerFilePath { get; set; }

    /// <summary>
    /// 调用方行号。
    /// </summary>
    [SugarColumn(ColumnName = "CallerLineNumber")]
    public int CallerLineNumber { get; set; }

    /// <summary>
    /// 调用方成员（方法名）。
    /// </summary>
    [SugarColumn(ColumnName = "CallerMember", IsNullable = true)]
    public string CallerMember { get; set; }
}