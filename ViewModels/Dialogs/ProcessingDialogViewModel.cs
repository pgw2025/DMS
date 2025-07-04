using CommunityToolkit.Mvvm.ComponentModel;

namespace PMSWPF.ViewModels.Dialogs;

public partial class ProcessingDialogViewModel : ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _message;
}
