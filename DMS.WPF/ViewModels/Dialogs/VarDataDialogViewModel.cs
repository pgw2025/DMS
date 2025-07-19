using CommunityToolkit.Mvvm.ComponentModel;
using DMS.WPF.Models;
using DMS.WPF.Models;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class VarDataDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private Variable _variable;
    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string primaryButtonText;
}