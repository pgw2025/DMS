using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Core.Enums;
using System.Collections.ObjectModel;
using System.Linq;

namespace DMS.WPF.ViewModels.Items;

public partial class VariableTableItemViewModel : ObservableObject
{
    public int Id { get; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private int _deviceId;

    [ObservableProperty]
    private ProtocolType _protocol;

    [ObservableProperty]
    private ObservableCollection<VariableItemViewModel> _variables = new();

    public VariableTableItemViewModel(VariableTableDto dto)
    {
        Id = dto.Id;
        _name = dto.Name;
        _description = dto.Description;
        _isActive = dto.IsActive;
        _deviceId = dto.DeviceId;
        _protocol = dto.Protocol;
        _variables = new ObservableCollection<VariableItemViewModel>(dto.Variables.Select(v => new VariableItemViewModel(v)));
    }
}
