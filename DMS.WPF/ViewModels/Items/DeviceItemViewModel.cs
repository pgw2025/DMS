// 文件: DMS.WPF/ViewModels/Items/DeviceItemViewModel.cs
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
    private string _opcUaServerUrl;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private string _status;

    public DeviceItemViewModel(DeviceDto dto)
    {
        Id = dto.Id;
        _name = dto.Name;
        _protocol = dto.Protocol;
        _ipAddress = dto.IpAddress;
        _port = dto.Port;
        _rack = dto.Rack;
        _slot = dto.Slot;
        _opcUaServerUrl = dto.OpcUaServerUrl;
        _isActive = dto.IsActive;
        _status = dto.Status;
    }

    public DeviceDto ToDto()
    {
        return new DeviceDto
        {
            Id = this.Id,
            Name = this.Name,
            Protocol = this.Protocol,
            IpAddress = this.IpAddress,
            Port = this.Port,
            Rack = this.Rack,
            Slot = this.Slot,
            OpcUaServerUrl = this.OpcUaServerUrl,
            IsActive = this.IsActive,
            Status = this.Status
        };
    }
}
