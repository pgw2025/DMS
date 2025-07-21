namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示菜单项的DTO。
/// </summary>
public class MenuBeanDto
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Header { get; set; }
    public string Icon { get; set; }
    public string TargetViewKey { get; set; }
    public string NavigationParameter { get; set; }
    public int DisplayOrder { get; set; }
}