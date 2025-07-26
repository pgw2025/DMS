using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Core.Enums;
using DMS.WPF.Services;

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
    private string _targetViewKey;

    [ObservableProperty]
    private string _navigationParameter;

    [ObservableProperty]
    private int _displayOrder;
    [ObservableProperty]
    private ObservableCollection<MenuBeanItemViewModel> _children=new ();
    
    /// <summary>
    /// 菜单项点击时执行的导航命令。
    /// </summary>
    public ICommand NavigateCommand { get; } 

    public MenuBeanItemViewModel(MenuBeanDto dto,INavigationService navigationService)
    {
        Id = dto.Id;
        _parentId = dto.ParentId;
        _header = dto.Header;
        _icon = dto.Icon;
        _menuType = dto.MenuType;
        _targetViewKey=dto.TargetViewKey;
        _targetId = dto.TargetId;
        _navigationParameter = dto.NavigationParameter;
        _displayOrder = dto.DisplayOrder;
        NavigateCommand = new AsyncRelayCommand(async () =>
        {
            await navigationService.NavigateToAsync(_targetViewKey, _navigationParameter);
        });
    }
}
