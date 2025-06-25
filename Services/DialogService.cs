using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Tools.Extension;
using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.Message;
using PMSWPF.Models;
using PMSWPF.ViewModels.Dialogs;
using PMSWPF.Views.Dialogs;

namespace PMSWPF.Services;

public class DialogService:ObservableRecipient,IRecipient<OpenDialogMessage> 
{
    public DialogService()
    {
        IsActive = true;
        
    }

    public async Task<Device> ShowAddDeviceDialog()
    {
        var device = new Device();
        var ddvm = new DeviceDialogViewModel(device)
        {
            Title = "添加设备"
        };

        var dialog = new DeviceDialog(ddvm);
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary) return device;

        return null;
    }

    public void ShowMessageDialog(string title, string message)
    {
        MessageBox.Show(message);
    }


    public void Receive(OpenDialogMessage message)
    {
        
        message.Reply(new DialogMessage(){IsConfirm = true, IsCancel = false});
    }
}