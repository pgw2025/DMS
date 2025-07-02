using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Enums;

namespace PMSWPF.Models;

public partial class VariableTable:ObservableObject
{
    public int Id { get; set; }
    [ObservableProperty] private string name;
    [ObservableProperty] private string description;
    
    public ProtocolType ProtocolType { get; set; }
    public List<VariableData> DataVariables { get; set; }
    [ObservableProperty]
    private bool isActive;
    public int? DeviceId { get; set; }
    public Device? Device { get; set; }
}