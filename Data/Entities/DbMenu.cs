using SqlSugar;

namespace PMSWPF.Data.Entities;

[SugarTable("Menu")]
public class DbMenu
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public  int Id { get; set; }
    public string Icon { get; set; }
    public string Name { get; set; }
    public int ParentId { get; set; }
    
    [SugarColumn(IsIgnore = true)]
    public List<DbMenu> Items { get; set; }
}