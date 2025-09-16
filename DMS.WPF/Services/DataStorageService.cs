using System.Collections.ObjectModel;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;
using ObservableCollections;

namespace DMS.WPF.Services;

public class DataStorageService : IDataStorageService
{


    /// <summary>
    /// 设备列表。
    /// </summary>
    public ObservableDictionary<int,DeviceItemViewModel> Devices { get; set; }

    /// <summary>
    /// 设备列表。
    /// </summary>
    public new ObservableDictionary<int,VariableTableItemViewModel> VariableTables { get; set; }

    /// <summary>
    /// 变量数据列表。
    /// </summary>
    public ObservableDictionary<int,VariableItemViewModel> Variables { get; set; }


    /// <summary>
    /// MQTT服务器列表。
    /// </summary>
    public ObservableCollection<MqttServerItemViewModel> MqttServers { get; set; }

    /// <summary>
    /// 菜单列表。
    /// </summary>
    public ObservableCollection<MenuItemViewModel> Menus { get; set; }

    /// <summary>
    /// 菜单树列表。
    /// </summary>
    public ObservableCollection<MenuItemViewModel> MenuTrees { get; set; }
    
    /// <summary>
    /// 日志列表。
    /// </summary>
    public ObservableCollection<NlogItemViewModel> Nlogs { get; set; }

    public DataStorageService()
    {
        Devices=new ObservableDictionary<int,DeviceItemViewModel>();
        VariableTables = new ObservableDictionary<int,VariableTableItemViewModel>();
        Variables=new ObservableDictionary<int,VariableItemViewModel>();
        MqttServers=new ObservableCollection<MqttServerItemViewModel>();
        Menus=new ObservableCollection<MenuItemViewModel>();
        MenuTrees=new ObservableCollection<MenuItemViewModel>();
        Nlogs=new ObservableCollection<NlogItemViewModel>();
        
    }

}
    
