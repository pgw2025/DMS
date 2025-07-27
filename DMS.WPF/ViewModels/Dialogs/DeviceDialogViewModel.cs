using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.WPF.ViewModels.Items;
using System.Threading.Tasks;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class DeviceDialogViewModel : DialogViewModelBase<DeviceItemViewModel>
{
    
    
    
    [ObservableProperty]
    private DeviceItemViewModel _device;

    public DeviceDialogViewModel(DeviceItemViewModel device)
    {
        _device = device;
    }

    [RelayCommand]
    private async Task Save()
    {
        // Here you can add validation logic before closing.
        await Close(Device);
    }

    [RelayCommand]
    private async Task Cancel()
    {
        await Close(null);
    }
}