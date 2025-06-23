using SqlSugar;

namespace PMSWPF.Data.Entities;

public class DbMqtt
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)] //数据库是自增才配自增 
    public int Id { get; set; }
}