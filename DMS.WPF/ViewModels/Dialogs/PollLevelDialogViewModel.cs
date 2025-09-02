using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Core.Enums;

namespace DMS.WPF.ViewModels.Dialogs
{
    public partial class PollLevelDialogViewModel : DialogViewModelBase<PollLevelType?>
    {
        [ObservableProperty]
        private PollLevelType _selectedPollLevelType;

        public List<PollLevelType> PollLevelTypes { get; }

        public PollLevelDialogViewModel(PollLevelType currentPollLevelType)
        {
            PollLevelTypes = Enum.GetValues(typeof(PollLevelType)).Cast<PollLevelType>().ToList();
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
