using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DMS.WPF.ViewModels.Items;

public partial class MqttServerItemViewModel : ObservableObject
{
    public int Id { get; }

    [ObservableProperty]
    private string _serverName;

    [ObservableProperty]
    private string _brokerAddress;

    [ObservableProperty]
    private int _port;

    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string _subscribeTopic;

    [ObservableProperty]
    private string _publishTopic;

    [ObservableProperty]
    private string _clientId;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime? _connectedAt;

    [ObservableProperty]
    private long _connectionDuration;

    [ObservableProperty]
    private string _messageFormat;

    [ObservableProperty]
    private ObservableCollection<VariableMqttAliasItemViewModel> _variableAliases = new();

    public MqttServerItemViewModel(MqttServerDto dto)
    {
        Id = dto.Id;
        _serverName = dto.ServerName;
        _brokerAddress = dto.BrokerAddress;
        _port = dto.Port;
        _username = dto.Username;
        _password = dto.Password;
        _isActive = dto.IsActive;
        _subscribeTopic = dto.SubscribeTopic;
        _publishTopic = dto.PublishTopic;
        _clientId = dto.ClientId;
        _createdAt = dto.CreatedAt;
        _connectedAt = dto.ConnectedAt;
        _connectionDuration = dto.ConnectionDuration;
        _messageFormat = dto.MessageFormat;
        _variableAliases = new ObservableCollection<VariableMqttAliasItemViewModel>(dto.VariableAliases.Select(va => new VariableMqttAliasItemViewModel(va)));
    }
}
