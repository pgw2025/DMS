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
    [ObservableProperty] private ObservableCollection<Device> _devices = new ();

    public DevicesViewModel(IDeviceDialogService deviceDialogService, DevicesRepositories devicesRepositories,INotificationService notificationService)
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
    public void Test()
    {
        // GrowlInfo info = new GrowlInfo();
        // info.Message = "Hello";
        // info.Type = InfoType.Error;
        // info.ShowCloseButton = true;
        _notificationService.Show( "Hello",NotificationType.Info,true);
        // Growl.Error("Hello");
        // Growl.Info("Hello");
        // Growl.Success("Hello");
        // Growl.WarningGlobal("Hello");
        // Growl.SuccessGlobal("Hello");
        // Growl.FatalGlobal("Hello");
        // Growl.Ask("Hello", isConfirmed =>
        // {
        //     Growl.Info(isConfirmed.ToString());
        //     return true;
        // });
        
        
    }

    [RelayCommand]
    public async void AddDevice()
    {
        try
        {
            Device device = await _deviceDialogService.ShowAddDeviceDialog();
            if (device != null)
            {
                DbDevice dbDevice = new DbDevice();
                device.CopyTo<DbDevice>(dbDevice);
                var rowCount = await _devicesRepositories.Add(dbDevice);
                if (rowCount > 0)
                {
                    // MessageBox.Show("Device added successfully");
                    await OnLoadedAsync();
                    _notificationService.Show(new Notification(){Message = "Hello World!",Type = NotificationType.Success,IsGlobal = true});
                }
            }
        }
        catch (DbExistException e)
        {
            Console.WriteLine(e);
            MessageBox.Show(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            MessageBox.Show(e.Message);
        }
    }

    public override void OnLoaded()
    {
        OnLoadedAsync().Await((e) => { _deviceDialogService.ShowMessageDialog("", e.Message); }, () => { });
    }
}