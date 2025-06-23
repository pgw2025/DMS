using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PMSWPF.Extensions;
using PMSWPF.Models;

namespace PMSWPF.ViewModels.Dialogs;

public partial class DeviceDialogViewModel : ObservableObject
{
    private readonly Device _saveDevice;

    [ObservableProperty] private Device device;

    [ObservableProperty] private string title = "添加设备";

    public DeviceDialogViewModel(Device saveDevice)
    {
        _saveDevice = saveDevice;
        device = new Device();
    }


    [RelayCommand]
    public void AddDevice()
    {
        device.CopyTo<Device>(_saveDevice);
    }
}