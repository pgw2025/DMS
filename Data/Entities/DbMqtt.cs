using SqlSugar;

namespace PMSWPF.Data.Entities;

[SugarTable("Mqtt")]
public class DbMqtt
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]//数据库是自增才配自增 
    public int Id { get; set; }
    
}