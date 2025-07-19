using System.Collections.ObjectModel;
using System.ComponentModel;
using DMS.Core.Enums;

namespace DMS.Core.Models;

/// <summary>
/// 表示变量表信息。
/// </summary>
public partial class VariableTable 
{
    /// <summary>
    /// 变量表关联的设备。
    /// </summary>
    public Device? Device { get; set; }

    /// <summary>
    /// 变量表关联的设备ID。
    /// </summary>
    public int? DeviceId { get; set; }

    /// <summary>
    /// 变量表描述。
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 变量表中包含的数据变量列表。
    /// </summary>
    public ObservableCollection<Variable> variables;
   
    /// <summary>
    /// 变量表的唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 表示变量表是否处于活动状态。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 变量表名称。
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 变量表使用的协议类型。
    /// </summary>
    public ProtocolType ProtocolType { get; set; }
}