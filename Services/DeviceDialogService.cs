using PMSWPF.Models;
using PMSWPF.ViewModels.Dialogs;
using PMSWPF.Views.Dialogs;

namespace PMSWPF.Services;

public class DeviceDialogService : IDeviceDialogService
{
    public async Task<Device> ShowAddDeviceDialog(Device device)
    {
        DeviceDialogViewModel ddvm = new DeviceDialogViewModel(device)
        {
            Title = "添加设备"
        };

        DeviceDialog dialog = new DeviceDialog(ddvm);
        await dialog.ShowAsync();
        return device;
    }
}