using System.Collections.ObjectModel;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.Services;

public class DataStorageService : IDataStorageService
{


    /// <summary>
    /// 设备列表。
    /// </summary>
    public ObservableCollection<DeviceItemViewModel> Devices { get; set; }

    /// <summary>
    /// 设备列表。
    /// </summary>
    public ObservableCollection<VariableTableItemViewModel> VariableTables { get; set; }

    /// <summary>
    /// 变量数据列表。
    /// </summary>
    public ObservableCollection<VariableItemViewModel> Variables { get; set; }


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
        Devices=new ObservableCollection<DeviceItemViewModel>();
        VariableTables = new ObservableCollection<VariableTableItemViewModel>();
        Variables=new ObservableCollection<VariableItemViewModel>();
        MqttServers=new ObservableCollection<MqttServerItemViewModel>();
        Menus=new ObservableCollection<MenuItemViewModel>();
        MenuTrees=new ObservableCollection<MenuItemViewModel>();
        Nlogs=new ObservableCollection<NlogItemViewModel>();
        
    }

}
    
