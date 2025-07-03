using PMSWPF.Enums;
using SqlSugar;
using SqlSugar.DbConvert;

namespace PMSWPF.Data.Entities;

/// <summary>
/// 表示数据库中的菜单实体。
/// </summary>
[SugarTable("Menu")]
public class DbMenu
{
    /// <summary>
    /// 菜单项关联的数据。
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public object? Data { get; set; }

    /// <summary>
    /// 菜单项关联的数据ID。
    /// </summary>
    public int DataId { get; set; }

    /// <summary>
    /// 菜单项的图标。
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// 菜单项的唯一标识符。
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 子菜单项列表。
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    public List<DbMenu> Items { get; set; }

    /// <summary>
    /// 菜单项的名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 父菜单项的ID。
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    /// 菜单项的类型。
    /// </summary>
    [SugarColumn(ColumnDataType = "varchar(20)", SqlParameterDbType = typeof(EnumToStringConvert))]
    public MenuType Type { get; set; }
}