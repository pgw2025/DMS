using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Models;

namespace DMS.ViewModels.Dialogs;

public partial class VarDataDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private Variable _variable;
    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string primaryButtonText;
}