using SqlSugar;

namespace PMSWPF.Data.Entities;

/// <summary>
/// 表示数据库中的用户实体。
/// </summary>
[SugarTable("User")]
public class DbUser
{
    /// <summary>
    /// 用户的唯一标识符。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增 
    public int Id { get; set; }
}