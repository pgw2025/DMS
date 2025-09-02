using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMS.WPF.ViewModels.Dialogs
{
    public partial class IsActiveDialogViewModel : DialogViewModelBase<bool?>
    {
        [ObservableProperty]
        private bool? _selectedIsActive;

        public IsActiveDialogViewModel(bool currentIsActive)
        {
            SelectedIsActive = currentIsActive;
            Title = "修改启用状态";
            PrimaryButText = "确定";
        }

        [RelayCommand]
        private void PrimaryButton()
        {
            Close(SelectedIsActive);
        }

        [RelayCommand]
        private void CancleButton()
        {
            Close(null);
        }
    }
}