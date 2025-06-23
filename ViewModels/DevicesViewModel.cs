using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Excptions;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

public partial class DevicesViewModel : ViewModelBase
{
    private readonly IDeviceDialogService _deviceDialogService;
    private readonly DevicesRepositories _devicesRepositories;
    private readonly ILogger<DevicesViewModel> _logger;

    [ObservableProperty] private ObservableCollection<Device> _devices;

    public DevicesViewModel(IDeviceDialogService deviceDialogService, DevicesRepositories devicesRepositories,
        ILogger<DevicesViewModel> logger
    )
    {
        _deviceDialogService = deviceDialogService;
        _devicesRepositories = devicesRepositories;
        _logger = logger;
    }

    public async Task OnLoadedAsync()
    {
        var ds = await _devicesRepositories.GetAll();
        Devices = new ObservableCollection<Device>(ds);
    }

    [RelayCommand]
    public async void AddDevice()
    {
        Device device = null;
        try
        {
            device = await _deviceDialogService.ShowAddDeviceDialog();
            if (device != null)
            {
                var isOk = await _devicesRepositories.Add(device);
                if (isOk)
                {
                    // MessageBox.Show("Device added successfully");
                    await OnLoadedAsync();
                    var msg = $"设备添加成功：{device.Name}";
                    _logger.LogInformation(msg);
                    NotificationHelper.ShowMessage(msg, NotificationType.Success);
                }
            }
        }
        catch (DbExistException e)
        {
            var msg = $"设备添加失败：名称为{device?.Name}的设备已经存在。请更换是被名称";
            _logger.LogError(msg);
            NotificationHelper.ShowMessage(msg, NotificationType.Error);
        }
        catch (Exception e)
        {
            var msg = $"添加设备的过程中发生错误：{e.Message}";
            _logger.LogError(msg);
            NotificationHelper.ShowMessage(msg, NotificationType.Success);
        }
    }

    [RelayCommand]
    public void NavigateVt()
    {
    }


    public override async void OnLoaded()
    {
        await OnLoadedAsync();
    }
}