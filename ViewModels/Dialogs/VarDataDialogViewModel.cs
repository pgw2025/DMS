using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Models;

namespace PMSWPF.ViewModels.Dialogs;

public partial class VarDataDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private VariableData variableData;
    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string primaryButtonText;
}