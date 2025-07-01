using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PMSWPF.Models;

namespace PMSWPF.ViewModels.Dialogs;

public partial class DeviceDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private Device _device;
    
    [ObservableProperty] private string title ;
    [ObservableProperty] private string primaryButContent ;

    public DeviceDialogViewModel(Device device)
    {
        _device = device;
    }


    [RelayCommand]
    public void AddDevice()
    {

    }
}