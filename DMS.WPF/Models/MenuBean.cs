using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Core.Enums;
using DMS.WPF.ViewModels;

namespace DMS.WPF.Models;

/// <summary>
/// 表示菜单项。
/// </summary>
public class MenuBean
{
    /// <summary>
    /// 菜单项关联的数据。
    /// </summary>
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
    public int Id { get; set; }

    /// <summary>
    /// 子菜单项列表。
    /// </summary>
    public List<MenuBean> Items { get; set; }

    /// <summary>
    /// 菜单项的名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 父菜单项。
    /// </summary>
    public MenuBean Parent { get; set; }

    /// <summary>
    /// 父菜单项的ID。
    /// </summary>
    public int ParentId { get; set; }

    /// <summary>
    /// 菜单项的类型。
    /// </summary>
    public MenuType Type { get; set; }

    /// <summary>
    /// 菜单项关联的ViewModel。
    /// </summary>
    public ViewModelBase ViewModel { get; set; }
}