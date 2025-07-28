using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;

namespace DMS.WPF.ViewModels.Items;

public partial class UserItemViewModel : ObservableObject
{
    public int Id { get; }

    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    private string _role;

    [ObservableProperty]
    private bool _isActive;

   
}
