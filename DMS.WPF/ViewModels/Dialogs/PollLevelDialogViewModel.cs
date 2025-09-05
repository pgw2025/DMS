using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMS.WPF.ViewModels.Dialogs
{
    public partial class PollLevelDialogViewModel : DialogViewModelBase<int?>
    {
        [ObservableProperty]
        private int _selectedPollLevelType;

        public List<int> PollLevelTypes { get; }

        public PollLevelDialogViewModel(int currentPollLevelType)
        {
            PollLevelTypes = new List<int> { 10, 100, 500, 1000, 5000, 10000, 20000, 30000, 60000, 180000, 300000, 600000, 1800000, 3600000 };
            SelectedPollLevelType = currentPollLevelType;
            Title = "修改轮询频率";
            PrimaryButText = "确定";
        }

        [RelayCommand]
        private void PrimaryButton()
        {
            Close(SelectedPollLevelType);
        }

        [RelayCommand]
        private void CancleButton()
        {
            Close(null);
        }
    }
}
