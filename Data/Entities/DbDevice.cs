using SqlSugar;

namespace PMSWPF.Data.Entities;

[SugarTable("Device")]
public class DbDevice
{
    
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]//数据库是自增才配自增 
    public int  Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Ip { get; set; }
    public bool IsActive { get; set; }
    public bool IsRuning { get; set; }
    
}