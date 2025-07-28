using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class ConfrimDialogViewModel : DialogViewModelBase<ConfrimDialogViewModel>
{
    public bool IsPrimaryButton { get; set; }
    
    [ObservableProperty]
    private string message;
    


    [RelayCommand]
    public void ParimaryButton()
    {
        IsPrimaryButton=true;
        Close(this);
    }
    [RelayCommand]
    public void CancleButton()
    {
        IsPrimaryButton=false;
        Close(this);
    }
}