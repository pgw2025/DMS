using SqlSugar;

namespace DMS.Infrastructure.Entities;

public class DbMenu
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    [SugarColumn(IsNullable = true)]
    public int? ParentId { get; set; }

    public string Header { get; set; }

    public string Icon { get; set; }

    public string TargetViewKey { get; set; }

    [SugarColumn(IsNullable = true)]
    public string NavigationParameter { get; set; }
    public List<DbMenu> Childrens { get; set; }

    public int DisplayOrder { get; set; }
}