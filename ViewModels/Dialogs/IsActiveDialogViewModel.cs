using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PMSWPF.Enums;

namespace PMSWPF.ViewModels.Dialogs;

public partial class IsActiveDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private bool? _selectedIsActive;

    public IsActiveDialogViewModel(bool? currentIsActive)
    {
        _selectedIsActive = currentIsActive;
    }

    [RelayCommand]
    private void SelectIsActive(bool isActive)
    {
        SelectedIsActive = isActive;
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedIsActive = null;
    }
}