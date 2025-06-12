using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.Models;
using PMSWPF.ViewModels.Dialogs;
using PMSWPF.Views.Dialogs;

namespace PMSWPF.Services;

public class DeviceDialogService : IDeviceDialogService
{
    public async Task<Device> ShowAddDeviceDialog()
    {
        Device device = new Device();
        DeviceDialogViewModel ddvm = new DeviceDialogViewModel(device)
        {
            Title = "添加设备"
        };

        DeviceDialog dialog = new DeviceDialog(ddvm);
        var res=await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            return device;
        }
        else
        {
            return null;
        }

    }
}