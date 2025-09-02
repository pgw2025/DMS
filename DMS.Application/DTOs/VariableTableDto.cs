using DMS.Core.Enums;

namespace DMS.Application.DTOs;

/// <summary>
/// 用于在UI上显示变量表基本信息的DTO。
/// </summary>
public class VariableTableDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; }
    public int DeviceId { get; set; }
    public DeviceDto Device { get; set; }
    public ProtocolType Protocol { get; set; }
    public List<VariableDto> Variables { get; set; } = new();
}