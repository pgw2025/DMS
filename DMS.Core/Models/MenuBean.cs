using DMS.Core.Enums;

namespace DMS.Core.Models;

/// <summary>
/// 菜单项领域模型
/// 代表系统中的一个菜单项，包含菜单的基本信息和导航相关信息
/// 作为领域模型，它在核心业务逻辑中使用，与数据库实体对应
/// </summary>
public class MenuBean
{
    /// <summary>
    /// 菜单项的唯一标识符
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 父级菜单项的ID，用于构建层级菜单结构
    /// 如果为null表示为顶级菜单项
    /// </summary>
    public int? ParentId { get; set; }
    
    /// <summary>
    /// 菜单项显示的标题文本
    /// </summary>
    public string Header { get; set; }
    
    /// <summary>
    /// 菜单项显示的图标资源路径或标识符
    /// </summary>
    public string Icon { get; set; }
    
    /// <summary>
    /// 菜单的类型,例如菜单关联的是设备，还是变量表，或者是MQTT
    /// 用于区分不同类型的菜单项，决定点击菜单项时的行为
    /// </summary>
    public MenuType MenuType { get; set; }
    
    /// <summary>
    /// 菜单关联的数据ID，例如设备Id，变量表Id
    /// 根据MenuType的不同，此ID可以指向不同的数据实体
    /// </summary>
    public int TargetId { get; set; }  
    
    /// <summary>
    /// 目标视图的键值，用于导航到特定的视图页面
    /// </summary>
    public string TargetViewKey { get; set; }
    
    /// <summary>
    /// 导航参数，传递给目标视图的额外参数信息
    /// </summary>
    public string NavigationParameter { get; set; }
    
    /// <summary>
    /// 菜单项在同级菜单中的显示顺序
    /// 数值越小显示越靠前
    /// </summary>
    public int DisplayOrder { get; set; }
}