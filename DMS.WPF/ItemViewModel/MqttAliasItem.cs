using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;

namespace DMS.WPF.ItemViewModel;

public partial class MqttAliasItem : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private int _variableId;

    [ObservableProperty]
    private int _mqttServerId;

    [ObservableProperty]
    private string _mqttServerName;

    [ObservableProperty]
    private string _alias;
    
    [ObservableProperty]
    private MqttServerItem _mqttServer;
    
    [ObservableProperty]
    private VariableItem _variable;
}
