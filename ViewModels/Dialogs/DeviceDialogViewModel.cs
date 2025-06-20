using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Models;

namespace PMSWPF.ViewModels.Dialogs;

public partial class DeviceDialogViewModel:ObservableObject
{
    private readonly Device _saveDevice;

    [ObservableProperty]
    private string title="添加设备";
    [ObservableProperty]
    private Device device;

    public DeviceDialogViewModel(Device saveDevice)
    {
        _saveDevice = saveDevice;
        this.device = new Device();
    }


    [RelayCommand]
    public void AddDevice()
    {
        this.device.CopyTo<Device>(_saveDevice);
    }
}