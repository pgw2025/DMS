// 文件: DMS.WPF/ViewModels/Items/DeviceItemViewModel.cs

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Events;
using DMS.WPF.Interfaces;

namespace DMS.WPF.ViewModels.Items;

/// <summary>
/// 代表设备列表中的单个设备项的ViewModel。
/// 实现了INotifyPropertyChanged，其任何属性变化都会自动通知UI。
/// </summary>
public partial class DeviceItemViewModel : ObservableObject
{
    // 用于访问事件服务的静态属性
    public static IEventService EventService { get; set; }
    
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

    partial void OnIsRunningChanged(bool oldValue, bool newValue)
    {
        System.Console.WriteLine($"IsRunning changed from {oldValue} to {newValue} for device {Name}");
    }

    /// <summary>
    /// 当IsActive属性改变时调用，用于发布设备状态改变事件
    /// </summary>
    partial void OnIsActiveChanged(bool oldValue, bool newValue)
    {
        // 只有当设备ID有效且事件服务已初始化时才发布事件
        if (Id > 0 && EventService != null )
        {
            // 发布设备状态改变事件（使用统一的事件类型）
            EventService.RaiseDeviceStateChanged(this, new DeviceStateChangedEventArgs(Id, Name, newValue, Core.Enums.DeviceStateType.Active));
        }
    }

    public ObservableCollection<VariableTableItemViewModel> VariableTables { get; set; } = new();
    
    [ObservableProperty]
    private bool _isConnected;
    
    [ObservableProperty]
    private string _connectionStatus = "未连接";

}
