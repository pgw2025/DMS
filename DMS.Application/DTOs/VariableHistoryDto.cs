using System;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示变量历史记录的DTO。
/// </summary>
public class VariableHistoryDto
{
    public long Id { get; set; }
    public int VariableId { get; set; }
    public string VariableName { get; set; }
    public string Value { get; set; }
    public double NumericValue { get; set; }
    public DateTime Timestamp { get; set; }
}