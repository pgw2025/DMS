using System.Collections.ObjectModel;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;
using DMS.WPF.ItemViewModel;
using ObservableCollections;

namespace DMS.WPF.Services;

public class DataStorageService : IDataStorageService
{


    /// <summary>
    /// 设备列表。
    /// </summary>
    public ObservableDictionary<int,DeviceItem> Devices { get; set; }

    /// <summary>
    /// 设备列表。
    /// </summary>
    public new ObservableDictionary<int,VariableTableItem> VariableTables { get; set; }

    /// <summary>
    /// 变量数据列表。
    /// </summary>
    public ObservableDictionary<int,VariableItem> Variables { get; set; }


    /// <summary>
    /// MQTT服务器列表。
    /// </summary>
    public ObservableDictionary<int, MqttServerItem> MqttServers { get; set; }

    /// <summary>
    /// 菜单列表。
    /// </summary>
    public ObservableCollection<MenuItem> Menus { get; set; }

    /// <summary>
    /// 菜单树列表。
    /// </summary>
    public ObservableCollection<MenuItem> MenuTrees { get; set; }
    
    /// <summary>
    /// 日志列表。
    /// </summary>
    public ObservableCollection<NlogItem> Nlogs { get; set; }

    /// <summary>
    /// 触发器列表。
    /// </summary>
    public ObservableDictionary<int, TriggerItem> Triggers { get; set; }

    public DataStorageService()
    {
        Devices=new ObservableDictionary<int,DeviceItem>();
        VariableTables = new ObservableDictionary<int,VariableTableItem>();
        Variables=new ObservableDictionary<int,VariableItem>();
        MqttServers=new ObservableDictionary<int, MqttServerItem>();
        Menus=new ObservableCollection<MenuItem>();
        MenuTrees=new ObservableCollection<MenuItem>();
        Nlogs=new ObservableCollection<NlogItem>();
        Triggers = new ObservableDictionary<int, TriggerItem>();
        
    }

}
    
