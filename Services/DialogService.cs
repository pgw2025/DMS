using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Tools.Extension;
using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.Message;
using PMSWPF.Models;
using PMSWPF.ViewModels.Dialogs;
using PMSWPF.Views.Dialogs;

namespace PMSWPF.Services;

public class DialogService :IDialogService
{
    public DialogService()
    {

    }

    public async Task<Device> ShowAddDeviceDialog()
    {
        var device = new Device();
        var dialog = new DeviceDialog(device);
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            return device;
        }
        return null;
    }

    public void ShowMessageDialog(string title, string message)
    {
        MessageBox.Show(message);
    }

}