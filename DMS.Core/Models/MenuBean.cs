namespace DMS.Core.Models;

/// <summary>
/// 领域模型：代表一个菜单项。
/// </summary>
public class MenuBean
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Header { get; set; }
    public string Icon { get; set; }
    public string TargetViewKey { get; set; }
    public string NavigationParameter { get; set; }
    public int DisplayOrder { get; set; }
}