using SqlSugar;

namespace DMS.Infrastructure.Entities;

[SugarTable("Variables")]
public class DbVariable
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public int DataType { get; set; } // 对应 SignalType 枚举
    public int PollLevel { get; set; } // 对应 PollLevelType 枚举
    public bool IsActive { get; set; }
    public int VariableTableId { get; set; }
    public string OpcUaNodeId { get; set; }
    public bool IsHistoryEnabled { get; set; }
    public double HistoryDeadband { get; set; }
    public bool IsAlarmEnabled { get; set; }
    public double AlarmMinValue { get; set; }
    public double AlarmMaxValue { get; set; }
    public double AlarmDeadband { get; set; }
    public int Protocol { get; set; } // 对应 ProtocolType 枚举
    public int CSharpDataType { get; set; } // 对应 CSharpDataType 枚举
    public string ConversionFormula { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
    public bool IsModified { get; set; }
}