using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Enums;

namespace DMS.Models;

/// <summary>
/// 表示变量表信息。
/// </summary>
public partial class VariableTable : ObservableObject
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
    [ObservableProperty]
    private string description;

    /// <summary>
    /// 变量表中包含的数据变量列表。
    /// </summary>
    [ObservableProperty]
    public ObservableCollection<Variable> variables;
   
    /// <summary>
    /// 变量表的唯一标识符。
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 表示变量表是否处于活动状态。
    /// </summary>
    [ObservableProperty]
    private bool isActive;

    /// <summary>
    /// 变量表名称。
    /// </summary>
    [ObservableProperty]
    private string name;

    /// <summary>
    /// 变量表使用的协议类型。
    /// </summary>
    public ProtocolType ProtocolType { get; set; }
}