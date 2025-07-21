using DMS.Core.Enums;
using System.Collections.Generic;

namespace DMS.Core.Models;

/// <summary>
/// 组织和管理一组相关的变量。
/// </summary>
public class VariableTable
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; set; } // 是否启用
    public int DeviceId { get; set; }
    public Device Device { get; set; }
    public ProtocolType Protocol { get; set; } // 通讯协议
    public List<Variable> Variables { get; set; } = new();
}