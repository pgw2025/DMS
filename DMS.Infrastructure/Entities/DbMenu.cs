using DMS.Core.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace DMS.Infrastructure.Entities;

/// <summary>
/// 数据库中的菜单项实体
/// </summary>
public class DbMenu
{
    /// <summary>
    /// 菜单的唯一标识符
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 父菜单的标识符。如果是根菜单，则为 null。
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public int? ParentId { get; set; }

    /// <summary>
    /// 菜单项显示的文本
    /// </summary>
    public string Header { get; set; }

    /// <summary>
    /// 与菜单项关联的图标
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 菜单的类型
    /// </summary>
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public MenuType MenuType { get; set; }
    
    /// <summary>
    /// 菜单关联的数据ID，例如设备Id，变量表Id
    /// </summary>
    public int TargetId { get; set; }
    /// <summary>
    /// 菜单关联的数据ID，例如设备Id，变量表Id
    /// </summary>
    public string TargetViewKey { get; set; }

    /// <summary>
    /// 导航的可选参数
    /// </summary>
    [SugarColumn(IsNullable = true)]
    public string NavigationParameter { get; set; }
    
    /// <summary>
    /// 子菜单项
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<DbMenu> Childrens { get; set; }

    /// <summary>
    /// 菜单项的显示顺序
    /// </summary>
    public int DisplayOrder { get; set; }
}