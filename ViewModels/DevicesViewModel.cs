using System.Windows;
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

    public DevicesViewModel(IDeviceDialogService deviceDialogService,DevicesRepositories devicesRepositories)
    {
        _deviceDialogService = deviceDialogService;
        _devicesRepositories = devicesRepositories;
    }


    [RelayCommand]
    public async void AddDevice()
    {
        try
        {
            Device device = new Device();
            await _deviceDialogService.ShowAddDeviceDialog(device);
            DbDevice dbDevice = new DbDevice();
            device.CopyTo<DbDevice>(dbDevice);
            await _devicesRepositories.Add(dbDevice);
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
}