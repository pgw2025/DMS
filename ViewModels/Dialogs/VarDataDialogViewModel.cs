using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Models;

namespace PMSWPF.ViewModels.Dialogs;

public partial class VarDataDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private Variable _variable;
    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string primaryButtonText;
}