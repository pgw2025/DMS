using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.WPF.ItemViewModel;
using System.Threading.Tasks;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class DeviceDialogViewModel : DialogViewModelBase<DeviceItem>
{
    [ObservableProperty]
    private bool _isAddMode;
    
    [ObservableProperty]
    private DeviceItem _device;

    public DeviceDialogViewModel(DeviceItem device=null)
    {
        if (device==null)
        {
            _device = new DeviceItem();
            IsAddMode=true;
        }
        else
        {
            _device=device;
         
        }
        
    }

    [RelayCommand]
    private async Task PrimaryButton()
    {
        await Close(Device);
    }

    [RelayCommand]
    private async Task CancleButton()
    {
        await Close(null);
    }
}