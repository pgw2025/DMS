using CommunityToolkit.Mvvm.ComponentModel;

namespace DMS.ViewModels.Dialogs;

public partial class ConfrimDialogViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string primaryButtonText;

    [ObservableProperty] private string message;
}