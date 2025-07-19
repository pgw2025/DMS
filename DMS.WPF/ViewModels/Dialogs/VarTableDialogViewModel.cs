using CommunityToolkit.Mvvm.ComponentModel;
using DMS.WPF.Models;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class VarTableDialogViewModel:ObservableObject
{
    [ObservableProperty] 
    private VariableTable variableTable;
    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string primaryButtonText;
}