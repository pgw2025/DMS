using DMS.Core.Enums;

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
    /// <summary>
    /// 菜单的类型,例如菜单关联的是设备，还是变量表，或者是MQTT
    /// </summary>
    public MenuType MenuType { get; set; }
    
    /// <summary>
    /// 菜单关联的数据ID，例如设备Id，变量表Id
    /// </summary>
    public int TargetId { get; set; } 
    /// <summary>
    /// 菜单关联的数据ID，例如设备Id，变量表Id
    /// </summary>
    public string TargetViewKey { get; set; }
    public string NavigationParameter { get; set; }
    public int DisplayOrder { get; set; }
}