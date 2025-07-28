using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;

namespace DMS.WPF.ViewModels.Items;

public partial class VariableMqttAliasItemViewModel : ObservableObject
{
    public int Id { get; }

    [ObservableProperty]
    private int _variableId;

    [ObservableProperty]
    private int _mqttServerId;

    [ObservableProperty]
    private string _mqttServerName;

    [ObservableProperty]
    private string _alias;

    
}
