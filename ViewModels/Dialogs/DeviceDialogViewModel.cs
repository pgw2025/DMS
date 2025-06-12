using CommunityToolkit.Mvvm.ComponentModel;

namespace PMSWPF.ViewModels.Dialogs;

public partial class DeviceDialogViewModel:ObservableObject
{
    [ObservableProperty]
    private string title="添加设备";
    public DeviceDialogViewModel()
    {
        
    }
}