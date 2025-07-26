using CommunityToolkit.Mvvm.ComponentModel;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class VarTableDialogViewModel:ObservableObject
{
    [ObservableProperty] 
    private VariableTableItemViewModel variableTable;
    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string primaryButtonText;
}