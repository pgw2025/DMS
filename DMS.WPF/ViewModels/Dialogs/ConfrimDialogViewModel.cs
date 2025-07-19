using CommunityToolkit.Mvvm.ComponentModel;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class ConfrimDialogViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string primaryButtonText;

    [ObservableProperty] private string message;
}