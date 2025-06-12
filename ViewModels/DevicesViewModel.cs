using CommunityToolkit.Mvvm.Input;
using PMSWPF.Models;
using PMSWPF.Services;
using PMSWPF.ViewModels.Dialogs;
using PMSWPF.Views.Dialogs;

namespace PMSWPF.ViewModels;

public partial class DevicesViewModel : ViewModelBase
{
    private readonly IDeviceDialogService _deviceDialogService;

    public DevicesViewModel(IDeviceDialogService deviceDialogService)
    {
        _deviceDialogService = deviceDialogService;
    }


    [RelayCommand]
    public async void AddDevice()
    {
        Device device = new Device();
        await _deviceDialogService.ShowAddDeviceDialog(device);
    }
}