using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using DMS.WPF.ItemViewModel;
using ObservableCollections;

namespace DMS.WPF.Interfaces;

public interface IWpfDataService
{
    /// <summary>
    /// 设备列表。
    /// </summary>
    ObservableDictionary<int, DeviceItem> Devices { get; set; }

    /// <summary>
    /// 设备列表。
    /// </summary>
    ObservableDictionary<int, VariableTableItem> VariableTables { get; set; }

    /// <summary>
    /// 变量数据列表。
    /// </summary>
    ObservableDictionary<int, VariableItem> Variables { get; set; }

    /// <summary>
    /// MQTT服务器列表。
    /// </summary>
    ObservableDictionary<int, MqttServerItem> MqttServers { get; set; }

    /// <summary>
    /// 菜单列表。
    /// </summary>
    ObservableCollection<MenuItem> Menus { get; set; }

    /// <summary>
    /// 菜单树列表。
    /// </summary>
    ObservableCollection<MenuItem> MenuTrees { get; set; }

    /// <summary>
    /// 日志列表。
    /// </summary>
    ObservableCollection<NlogItem> Nlogs { get; set; }

    /// <summary>
    /// MQTT别名列表。
    /// </summary>
    ObservableDictionary<int, MqttAliasItem> MqttAliases { get; set; }

    /// <summary>
    /// 触发器列表。
    /// </summary>
    ObservableDictionary<int, TriggerItem> Triggers { get; set; }
}