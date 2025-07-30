using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Core.Enums;
using DMS.WPF.Services;

namespace DMS.WPF.ViewModels.Items;

public partial class MenuItemViewModel : ObservableObject
{
    public int Id { get; set; }

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
    private ObservableCollection<MenuItemViewModel> _children=new ();
    

}
