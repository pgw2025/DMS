using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using DMS.WPF.ViewModels.Items;
using ObservableCollections;

namespace DMS.WPF.Interfaces;

public interface IDataStorageService
{
    /// <summary>
    /// 设备列表。
    /// </summary>
    ObservableDictionary<int, DeviceItemViewModel> Devices { get; set; }

    /// <summary>
    /// 设备列表。
    /// </summary>
    ObservableDictionary<int, VariableTableItemViewModel> VariableTables { get; set; }

    /// <summary>
    /// 变量数据列表。
    /// </summary>
    ObservableDictionary<int, VariableItemViewModel> Variables { get; set; }

    /// <summary>
    /// MQTT服务器列表。
    /// </summary>
    ObservableDictionary<int, MqttServerItemViewModel> MqttServers { get; set; }

    /// <summary>
    /// 菜单列表。
    /// </summary>
    ObservableCollection<MenuItemViewModel> Menus { get; set; }

    /// <summary>
    /// 菜单树列表。
    /// </summary>
    ObservableCollection<MenuItemViewModel> MenuTrees { get; set; }

    /// <summary>
    /// 日志列表。
    /// </summary>
    ObservableCollection<NlogItemViewModel> Nlogs { get; set; }

    /// <summary>
    /// 触发器列表。
    /// </summary>
    ObservableDictionary<int, TriggerItemViewModel> Triggers { get; set; }
}