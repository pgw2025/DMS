using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Enums;

namespace DMS.ViewModels.Dialogs;

public partial class IsActiveDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private bool? _selectedIsActive;

    public IsActiveDialogViewModel(bool? currentIsActive)
    {
        _selectedIsActive = currentIsActive;
    }

    [RelayCommand]
    private void SelectIsActive(string isActiveString)
    {
        if (bool.TryParse(isActiveString, out bool isActive))
        {
            SelectedIsActive = isActive;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        SelectedIsActive = null;
    }
}