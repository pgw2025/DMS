using SqlSugar;

namespace PMSWPF.Data.Entities;

[SugarTable("User")]
public class DbUser
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增 
    public int Id { get; set; }
}