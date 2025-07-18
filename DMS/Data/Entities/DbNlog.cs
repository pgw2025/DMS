using SqlSugar;

namespace DMS.Data.Entities;

/// <summary>
/// 表示数据库中的NLog日志实体。
/// </summary>
[SugarTable("nlog")]
public class DbNlog
{
    /// <summary>
    /// 日志调用位置。
    /// </summary>
    public string Callsite { get; set; }

    /// <summary>
    /// 日志调用行号。
    /// </summary>
    public int CallsiteLineNumber { get; set; }

    /// <summary>
    /// 异常信息。
    /// </summary>
    [SugarColumn(IsNullable = true, ColumnDataType = "text")]
    public string Exception { get; set; }

    /// <summary>
    /// 日志的唯一标识符。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增 
    public int Id { get; set; }

    /// <summary>
    /// 日志级别。
    /// </summary>
    public string Level { get; set; }

    /// <summary>
    /// 日志记录器名称。
    /// </summary>
    public string Logger { get; set; }

    /// <summary>
    /// 日志时间。
    /// </summary>
    public DateTime LogTime { get; set; }

    /// <summary>
    /// 日志消息内容。
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 线程ID。
    /// </summary>
    public int ThreadID { get; set; }

    /// <summary>
    /// 线程名称。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string ThreadName { get; set; }

    /// <summary>
    /// 线程名称。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string CallerFilePath { get; set; }
    /// <summary>
    /// 线程名称。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public int CallerLineNumber { get; set; }
    /// <summary>
    /// 线程名称。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string CallerMember { get; set; }
}