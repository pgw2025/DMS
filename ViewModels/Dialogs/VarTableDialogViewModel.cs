using CommunityToolkit.Mvvm.ComponentModel;
using PMSWPF.Models;

namespace PMSWPF.ViewModels.Dialogs;

public partial class VarTableDialogViewModel:ObservableObject
{
    [ObservableProperty] 
    private VariableTable variableTable;
    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string primaryButtonText;
}