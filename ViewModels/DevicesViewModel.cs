using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using PMSWPF.Data.Entities;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Excptions;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.Services;
using PMSWPF.ViewModels.Dialogs;
using PMSWPF.Views.Dialogs;
using MessageBox = System.Windows.MessageBox;
using Notification = PMSWPF.Models.Notification;

namespace PMSWPF.ViewModels;

public partial class DevicesViewModel : ViewModelBase
{
    private readonly IDeviceDialogService _deviceDialogService;
    private readonly DevicesRepositories _devicesRepositories;
    private readonly INotificationService _notificationService;
    [ObservableProperty] private ObservableCollection<Device> _devices = new();

    public DevicesViewModel(IDeviceDialogService deviceDialogService, DevicesRepositories devicesRepositories,
        INotificationService notificationService)
    {
        _deviceDialogService = deviceDialogService;
        _devicesRepositories = devicesRepositories;
        _notificationService = notificationService;
    }

    public async Task OnLoadedAsync()
    {
        var ds = await _devicesRepositories.GetAll();
        _devices.Clear();
        foreach (var dbDevice in ds)
        {
            Device device = new Device();
            dbDevice.CopyTo(device);
            _devices.Add(device);
        }
    }

    [RelayCommand]
    public async  void AddDevice()
    {
        Device device = null;
        try
        {
            device= await _deviceDialogService.ShowAddDeviceDialog();
            if (device != null)
            {
                var isOk = await _devicesRepositories.Add(device);
                if (isOk)
                {
                    // MessageBox.Show("Device added successfully");
                    await OnLoadedAsync();
                    _notificationService.Show($"设备添加成功：{device.Name}", NotificationType.Success);
                }
            }
        }
        catch (DbExistException e)
        {
            _notificationService.Show($"设备添加失败：名称为{device?.Name}的设备已经存在。请更换是被名称", NotificationType.Error);
        }
        catch (Exception e)
        {
            _notificationService.Show($"添加设备的过程中发生错误：{e.Message}", NotificationType.Error);
        }
    }

    public override void OnLoaded()
    {
        OnLoadedAsync().Await((e) => { _deviceDialogService.ShowMessageDialog("", e.Message); }, () => { });
    }
}