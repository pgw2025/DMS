using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DMS.Core.Enums;

namespace DMS.WPF.ViewModels.Dialogs
{
    public partial class PollLevelDialogViewModel : ObservableObject
    {
        [ObservableProperty]
        private PollLevelType _selectedPollLevelType;

        public List<PollLevelType> PollLevelTypes { get; }

        public PollLevelDialogViewModel(PollLevelType currentPollLevelType)
        {
            PollLevelTypes = Enum.GetValues(typeof(PollLevelType)).Cast<PollLevelType>().ToList();
            SelectedPollLevelType = currentPollLevelType;
        }
    }
}
