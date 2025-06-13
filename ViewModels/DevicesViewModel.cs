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
        _devices = new ObservableCollection<Device>();
    }

    public async Task OnLoadedAsync()
    {
        var ds = await _devicesRepositories.GetAll();
  
        foreach (var dbDevice in ds)
        {
           var deviceExist= _devices.FirstOrDefault(d => d.Id == dbDevice.Id);
           if (deviceExist == null)
           {
               Device device = new Device();
               dbDevice.CopyTo(device);
               _devices.Add(device);
           }
            
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
                 // MessageBox.Show("Device added successfully");
                 await OnLoadedAsync();
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