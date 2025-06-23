using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Extensions.Logging;
using PMSWPF.Data.Entities;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Excptions;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Message;
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
    private readonly ILogger<DevicesViewModel> _logger;
    [ObservableProperty] 
    private ObservableCollection<Device> _devices = new();

    public DevicesViewModel(IDeviceDialogService deviceDialogService, DevicesRepositories devicesRepositories,ILogger<DevicesViewModel> logger
       )
    {
        _deviceDialogService = deviceDialogService;
        _devicesRepositories = devicesRepositories;
        _logger = logger;
    }

    public async Task OnLoadedAsync()
    {
        var ds = await _devicesRepositories.GetAll();
        _devices.Clear();
        foreach (var dbDevice in ds)
        {
            Device device = new Device();
            dbDevice.CopyTo(device);
            foreach (var dbVariableTable in dbDevice.VariableTables)
            {

                if (device.VariableTables == null)
                {
                    device.VariableTables=new List<VariableTable>();
                }

                var table = dbVariableTable.NewTo<VariableTable>();
                device.VariableTables.Add(table);
            }
            
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

    public override async void OnLoaded()
    {
        // OnLoadedAsync().Await((e) => { _deviceDialogService.ShowMessageDialog("", e.Message); }, () => { });
        await OnLoadedAsync();
    }
}