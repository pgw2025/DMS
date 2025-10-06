using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;

namespace DMS.WPF.ItemViewModel;

public partial class UserItem : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    private string _role;

    [ObservableProperty]
    private bool _isActive;

   
}
