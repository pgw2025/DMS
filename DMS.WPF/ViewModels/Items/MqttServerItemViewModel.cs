using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace DMS.WPF.ViewModels.Items;

public partial class MqttServerItemViewModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _serverName;

    [ObservableProperty]
    private string _serverUrl;

    [ObservableProperty]
    private int _port;

    [ObservableProperty]
    private string _username;

    [ObservableProperty]
    private string _password;

    [ObservableProperty]
    private bool _isActive;
    
    [ObservableProperty]
    private bool _isConnect;

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
    private string _messageHeader;

    [ObservableProperty]
    private string _messageContent;

    [ObservableProperty]
    private string _messageFooter;

    [ObservableProperty]
    private ObservableCollection<MqttAliasItem> _variableAliases = new();


}
