using CommunityToolkit.Mvvm.ComponentModel;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class VarDataDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private VariableItemViewModel _variable;
    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string primaryButtonText;
}