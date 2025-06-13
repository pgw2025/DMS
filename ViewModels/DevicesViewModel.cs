using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PMSWPF.Data.Entities;
using PMSWPF.Data.Repositories;
using PMSWPF.Excptions;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.Services;
using PMSWPF.ViewModels.Dialogs;
using PMSWPF.Views.Dialogs;

namespace PMSWPF.ViewModels;

public partial class DevicesViewModel : ViewModelBase
{
    private readonly IDeviceDialogService _deviceDialogService;
    private readonly DevicesRepositories _devicesRepositories;
    [ObservableProperty]
    private ObservableCollection<Device> _devices;
    public DevicesViewModel(IDeviceDialogService deviceDialogService,DevicesRepositories devicesRepositories)
    {
        _deviceDialogService = deviceDialogService;
        _devicesRepositories = devicesRepositories;
    }

    public async Task OnLoadedAsync()
    {
        var ds = await _devicesRepositories.GetAll();
        _devices = new ObservableCollection<Device>();
        
        foreach (var dbDevice in ds)
        {
            Device device = new Device();
            dbDevice.CopyTo(device);
            _devices.Add(device);
        }
    }


    [RelayCommand]
    public async void AddDevice()
    {
        try
        {
            
          Device device=  await _deviceDialogService.ShowAddDeviceDialog();
          if (device != null)
          {
              DbDevice dbDevice = new DbDevice();
              device.CopyTo<DbDevice>(dbDevice);
             var rowCount= await _devicesRepositories.Add(dbDevice);
             if (rowCount>0)
             {
                 MessageBox.Show("Device added successfully");
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
        OnLoadedAsync().Await((e) =>
        {
            _deviceDialogService.ShowMessageDialog("",e.Message);
        }, () =>
        {

        });
    }
}