using DMS.Core.Enums;
using SqlSugar;
using SqlSugar.DbConvert;
using CSharpDataType = SqlSugar.CSharpDataType;

namespace DMS.Infrastructure.Entities;

public class DbVariable
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public SignalType SignalType { get; set; } // 对应 SignalType 枚举
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public PollLevelType PollLevel { get; set; } // 对应 PollLevelType 枚举
    public bool IsActive { get; set; }
    public int VariableTableId { get; set; }
    public string DataValue { get; set; }
    public string DisplayValue { get; set; }
    public string S7Address { get; set; }
    public string OpcUaNodeId { get; set; }
    public bool IsHistoryEnabled { get; set; }
    public double HistoryDeadband { get; set; }
    public bool IsAlarmEnabled { get; set; }
    public double AlarmMinValue { get; set; }
    public double AlarmMaxValue { get; set; }
    public double AlarmDeadband { get; set; }
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public ProtocolType Protocol { get; set; } // 对应 ProtocolType 枚举
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public CSharpDataType CSharpDataType { get; set; } // 对应 CSharpDataType 枚举
    public string ConversionFormula { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsModified { get; set; }
    [SugarColumn(ColumnDataType="varchar(20)",SqlParameterDbType=typeof(EnumToStringConvert))]
    public OpcUaUpdateType OpcUaUpdateType { get; set; }
}