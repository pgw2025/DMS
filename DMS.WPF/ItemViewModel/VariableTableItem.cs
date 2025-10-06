using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Core.Enums;
using System.Collections.ObjectModel;
using System.Linq;

namespace DMS.WPF.ItemViewModel;

public partial class VariableTableItem : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private bool _isActive=true;

    [ObservableProperty]
    private int _deviceId;
    
    [ObservableProperty]
    private DeviceItem _device;

    [ObservableProperty]
    private ProtocolType _protocol;

    [ObservableProperty]
    private ObservableCollection<VariableItem> _variables = new();

   
}
