using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class ConfrimDialogViewModel : DialogViewModelBase<Boolean>
{
    
    [ObservableProperty]
    private string _message;

    public ConfrimDialogViewModel(string title,string message,string primaryButText)
    {
        Message = message;
        Title = title;
        PrimaryButText = primaryButText;
    }


    [RelayCommand]
    private void PrimaryButton()
    {
        Close(true);
    }
    [RelayCommand]
    private void CancleButton()
    {
        Close(false);
    }
}