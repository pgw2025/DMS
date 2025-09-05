using DMS.Core.Enums;
using System;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示变量基本信息的DTO。
/// </summary>
public class VariableDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string S7Address { get; set; }
    public string DataValue { get; set; }
    public string DisplayValue { get; set; }
    public VariableTableDto? VariableTable { get; set; }
    public List<VariableMqttAliasDto>? MqttAliases { get; set; }
    public SignalType SignalType { get; set; }
    public int PollLevel { get; set; }
    public bool IsActive { get; set; }
    public int VariableTableId { get; set; }
    public string OpcUaNodeId { get; set; }
    public bool IsHistoryEnabled { get; set; }
    public double HistoryDeadband { get; set; }
    public bool IsAlarmEnabled { get; set; }
    public double AlarmMinValue { get; set; }
    public double AlarmMaxValue { get; set; }
    public double AlarmDeadband { get; set; }
    public ProtocolType Protocol { get; set; }
    public DataType DataType { get; set; }
    public string ConversionFormula { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsModified { get; set; }
    public string Description { get; set; }
    public OpcUaUpdateType OpcUaUpdateType { get; set; }
}