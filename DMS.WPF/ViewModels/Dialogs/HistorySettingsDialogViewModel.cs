using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMS.WPF.ViewModels.Dialogs
{
    public partial class HistorySettingsDialogViewModel : DialogViewModelBase<HistorySettingsResult>
    {
        [ObservableProperty]
        private bool _isHistoryEnabled;

        [ObservableProperty]
        private double _historyDeadband;

        public HistorySettingsDialogViewModel(bool currentIsHistoryEnabled, double currentHistoryDeadband)
        {
            IsHistoryEnabled = currentIsHistoryEnabled;
            HistoryDeadband = currentHistoryDeadband;
            Title = "修改历史记录设置";
            PrimaryButText = "确定";
        }

        [RelayCommand]
        private void PrimaryButton()
        {
            var result = new HistorySettingsResult
            {
                IsHistoryEnabled = IsHistoryEnabled,
                HistoryDeadband = HistoryDeadband
            };
            Close(result);
        }

        [RelayCommand]
        private void CancleButton()
        {
            Close(null);
        }
    }

    public class HistorySettingsResult
    {
        public bool IsHistoryEnabled { get; set; }
        public double HistoryDeadband { get; set; }
    }
}