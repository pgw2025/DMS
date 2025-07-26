using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Core.Enums;

namespace DMS.WPF.ViewModels.Items;

public partial class MenuBeanItemViewModel : ObservableObject
{
    public int Id { get; }

    [ObservableProperty]
    private int? _parentId;

    [ObservableProperty]
    private string _header;

    [ObservableProperty]
    private string _icon;

    [ObservableProperty]
    private MenuType _menuType;

    [ObservableProperty]
    private int _targetId;

    [ObservableProperty]
    private string _navigationParameter;

    [ObservableProperty]
    private int _displayOrder;

    public MenuBeanItemViewModel(MenuBeanDto dto)
    {
        Id = dto.Id;
        _parentId = dto.ParentId;
        _header = dto.Header;
        _icon = dto.Icon;
        _menuType = dto.MenuType;
        _targetId = dto.TargetId;
        _navigationParameter = dto.NavigationParameter;
        _displayOrder = dto.DisplayOrder;
    }
}
