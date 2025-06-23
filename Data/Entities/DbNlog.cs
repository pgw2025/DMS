using SqlSugar;

namespace PMSWPF.Data.Entities;

[SugarTable("nlog")]
public class DbNlog
{
    // INSERT INTO [dbo].[NLog] (
    // [Application], [Logged], [Level], [ThreadID],[Message],
    // [Logger], [Callsite], [Exception], [Url], [Action], [User]
    // ) VALUES (
    //     @Application, @Logged, @Level,@ThreadID, @Message,
    //     @Logger, @Callsite, @Exception, @Url, @Action, @User
    // )
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增 
    public int Id { get; set; }

    public DateTime LogTime { get; set; }
    public string Level { get; set; }
    public int ThreadID { get; set; }

    [SugarColumn(IsNullable = true)] public string ThreadName { get; set; }

    public string Logger { get; set; }
    public string Callsite { get; set; }
    public int CallsiteLineNumber { get; set; }
    public string Message { get; set; }

    [SugarColumn(IsNullable = true, ColumnDataType = "text")]
    public string Exception { get; set; }
}