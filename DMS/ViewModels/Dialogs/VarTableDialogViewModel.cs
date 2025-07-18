using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Models;

namespace DMS.ViewModels.Dialogs;

public partial class VarTableDialogViewModel:ObservableObject
{
    [ObservableProperty] 
    private VariableTable variableTable;
    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string primaryButtonText;
}