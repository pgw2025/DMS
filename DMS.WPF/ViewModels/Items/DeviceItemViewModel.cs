// 文件: DMS.WPF/ViewModels/Items/DeviceItemViewModel.cs

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Core.Enums;

namespace DMS.WPF.ViewModels.Items;

/// <summary>
/// 代表设备列表中的单个设备项的ViewModel。
/// 实现了INotifyPropertyChanged，其任何属性变化都会自动通知UI。
/// </summary>
public partial class DeviceItemViewModel : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private ProtocolType _protocol;

    [ObservableProperty]
    private string _ipAddress;

    [ObservableProperty]
    private int _port=102;

    [ObservableProperty]
    private int _rack;

    [ObservableProperty]
    private int _slot=1;

    [ObservableProperty]
    private CpuType _cpuType;

    [ObservableProperty]
    private DeviceType _deviceType;

    [ObservableProperty]
    private string _opcUaServerUrl;

    [ObservableProperty]
    private bool _isActive =true;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _status;
    
    [ObservableProperty]
    private bool _isAddDefVarTable=true;

    partial void OnIpAddressChanged(string newIpAddress)
    {
        if (Protocol == ProtocolType.OpcUa)
        {
            OpcUaServerUrl="opc.tcp://" + IpAddress+":"+Port;
        }
    }

    partial void OnPortChanged(int newPort)
    {
        if (Protocol == ProtocolType.OpcUa)
        {
            OpcUaServerUrl="opc.tcp://" + IpAddress+":"+Port;
        }
    }

    public ObservableCollection<VariableTableItemViewModel> VariableTables { get; set; } = new();

}
