using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Core.Enums;
using System;
using System.Collections.Generic;

namespace DMS.WPF.ViewModels.Items;

public partial class VariableItemViewModel : ObservableObject
{
    public int Id { get; }

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
    private SignalType _signalType;

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

    public VariableItemViewModel(VariableDto dto)
    {
        Id = dto.Id;
        _name = dto.Name;
        _s7Address = dto.S7Address;
        _dataValue = dto.DataValue;
        _displayValue = dto.DisplayValue;
        _variableTable = dto.VariableTable;
        _mqttAliases = dto.MqttAliases;
        _signalType = dto.SignalType;
        _pollLevel = dto.PollLevel;
        _isActive = dto.IsActive;
        _variableTableId = dto.VariableTableId;
        _opcUaNodeId = dto.OpcUaNodeId;
        _isHistoryEnabled = dto.IsHistoryEnabled;
        _historyDeadband = dto.HistoryDeadband;
        _isAlarmEnabled = dto.IsAlarmEnabled;
        _alarmMinValue = dto.AlarmMinValue;
        _alarmMaxValue = dto.AlarmMaxValue;
        _alarmDeadband = dto.AlarmDeadband;
        _protocol = dto.Protocol;
        _cSharpDataType = dto.CSharpDataType;
        _conversionFormula = dto.ConversionFormula;
        _createdAt = dto.CreatedAt;
        _updatedAt = dto.UpdatedAt;
        _updatedBy = dto.UpdatedBy;
        _isModified = dto.IsModified;
        _description = dto.Description;
        _opcUaUpdateType = dto.OpcUaUpdateType;
    }
}
