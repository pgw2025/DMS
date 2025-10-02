namespace DMS.Core.Models;

/// <summary>
/// 用于存储变量值的变化记录。
/// </summary>
public class VariableHistory
{
    public long Id { get; set; }
    public int VariableId { get; set; }
    public Variable Variable { get; set; }
    public string Value { get; set; } // 以字符串形式存储，便于通用性
    public double NumericValue { get; set; }
    public DateTime Timestamp { get; set; }
}