using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Core.Enums;
using System;
using System.Collections.Generic;

namespace DMS.WPF.ViewModels.Items;

public partial class VariableItemViewModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string? _s7Address;

    [ObservableProperty]
    private string? _dataValue;

    [ObservableProperty]
    private string? _displayValue;

    [ObservableProperty]
    private VariableTableDto? _variableTable;

    [ObservableProperty]
    private List<VariableMqttAliasDto>? _mqttAliases;

    [ObservableProperty]
    private SignalType _signalType ;

    [ObservableProperty]
    private PollLevelType _pollLevel;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private int _variableTableId;

    [ObservableProperty]
    private string _opcUaNodeId;

    [ObservableProperty]
    private bool _isHistoryEnabled;

    [ObservableProperty]
    private double _historyDeadband;

    [ObservableProperty]
    private bool _isAlarmEnabled;

    [ObservableProperty]
    private double _alarmMinValue;

    [ObservableProperty]
    private double _alarmMaxValue;

    [ObservableProperty]
    private double _alarmDeadband;

    [ObservableProperty]
    private ProtocolType _protocol;

    [ObservableProperty]
    private CSharpDataType _cSharpDataType;

    [ObservableProperty]
    private string _conversionFormula;

    [ObservableProperty]
    private DateTime _createdAt;

    [ObservableProperty]
    private DateTime _updatedAt;

    [ObservableProperty]
    private string _updatedBy;

    [ObservableProperty]
    private bool _isModified;

    [ObservableProperty]
    private string _description;


    [ObservableProperty]
    private OpcUaUpdateType _opcUaUpdateType;

    
}
