using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Core.Enums;
using DMS.WPF.Services;

namespace DMS.WPF.ItemViewModel;

/// <summary>
/// 菜单项视图模型
/// 用于在WPF界面中绑定和显示菜单项数据，实现MVVM模式
/// 继承自ObservableObject以支持属性更改通知
/// </summary>
public partial class MenuItem : ObservableObject
{
    /// <summary>
    /// 菜单项的唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 父级菜单项的ID，用于构建层级菜单结构
    /// 如果为null表示为顶级菜单项
    /// </summary>
    [ObservableProperty]
    private int? _parentId;

    /// <summary>
    /// 菜单项显示的标题文本
    /// </summary>
    [ObservableProperty]
    private string _header;

    /// <summary>
    /// 菜单项显示的图标资源路径或标识符
    /// </summary>
    [ObservableProperty]
    private string _icon;

    /// <summary>
    /// 菜单的类型,例如菜单关联的是设备，还是变量表，或者是MQTT
    /// 用于区分不同类型的菜单项，决定点击菜单项时的行为
    /// </summary>
    [ObservableProperty]
    private MenuType _menuType;

    /// <summary>
    /// 菜单关联的数据ID，例如设备Id，变量表Id
    /// 根据MenuType的不同，此ID可以指向不同的数据实体
    /// </summary>
    [ObservableProperty]
    private int _targetId;
    
    /// <summary>
    /// 目标视图的键值，用于导航到特定的视图页面
    /// </summary>
    [ObservableProperty]
    private string _targetViewKey;

    /// <summary>
    /// 导航参数，传递给目标视图的额外参数信息
    /// </summary>
    [ObservableProperty]
    private string _navigationParameter;

    /// <summary>
    /// 菜单项在同级菜单中的显示顺序
    /// 数值越小显示越靠前
    /// </summary>
    [ObservableProperty]
    private int _displayOrder;
    
    /// <summary>
    /// 子菜单项集合，用于构建层级菜单结构
    /// 使用ObservableCollection以支持动态添加和删除子项并自动更新UI
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<MenuItem> _children = new();
}