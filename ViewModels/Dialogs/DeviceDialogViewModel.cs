using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PMSWPF.Extensions;
using PMSWPF.Models;

namespace PMSWPF.ViewModels.Dialogs;

public partial class DeviceDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private Device _device;


    [ObservableProperty] private string title = "添加设备";

    public DeviceDialogViewModel(Device device)
    {
        _device = device;
    }


    [RelayCommand]
    public void AddDevice()
    {

    }
}