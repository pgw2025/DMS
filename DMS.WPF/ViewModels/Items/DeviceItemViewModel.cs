// 文件: DMS.WPF/ViewModels/Items/DeviceItemViewModel.cs

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Application.DTOs;
using DMS.Core.Enums;

namespace DMS.WPF.ViewModels.Items;

/// <summary>
/// 代表设备列表中的单个设备项的ViewModel。
/// 实现了INotifyPropertyChanged，其任何属性变化都会自动通知UI。
/// </summary>
public partial class DeviceItemViewModel : ObservableObject
{
    public int Id { get; }

    [ObservableProperty]
    private string _name;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private ProtocolType _protocol;

    [ObservableProperty]
    private string _ipAddress;

    [ObservableProperty]
    private int _port;

    [ObservableProperty]
    private int _rack;

    [ObservableProperty]
    private int _slot;

    [ObservableProperty]
    private CpuType _cpuType;

    [ObservableProperty]
    private DeviceType _deviceType;

    [ObservableProperty]
    private string _opcUaServerUrl;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private bool _isRunning;

    [ObservableProperty]
    private string _status;

    public ObservableCollection<VariableTableItemViewModel> VariableTables { get; set; } = new();

    public DeviceItemViewModel(DeviceDto dto)
    {
        Id = dto.Id;
        _name = dto.Name;
        _description = dto.Description;
        _protocol = dto.Protocol;
        _ipAddress = dto.IpAddress;
        _port = dto.Port;
        _rack = dto.Rack;
        _slot = dto.Slot;
        _cpuType = dto.CpuType;
        _deviceType = dto.DeviceType;
        _opcUaServerUrl = dto.OpcUaServerUrl;
        _isActive = dto.IsActive;
        _isRunning = dto.IsRunning;
        _status = dto.Status;
    }

    public DeviceItemViewModel()
    {
        
    }

    public DeviceDto ToDto()
    {
        return new DeviceDto
        {
            Id = this.Id,
            Name = this.Name,
            Description = this.Description,
            Protocol = this.Protocol,
            IpAddress = this.IpAddress,
            Port = this.Port,
            Rack = this.Rack,
            Slot = this.Slot,
            CpuType = this.CpuType,
            DeviceType = this.DeviceType,
            OpcUaServerUrl = this.OpcUaServerUrl,
            IsActive = this.IsActive,
            IsRunning = this.IsRunning,
            Status = this.Status
        };
    }
}
