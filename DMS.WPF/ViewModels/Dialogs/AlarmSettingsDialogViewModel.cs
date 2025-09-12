using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMS.WPF.ViewModels.Dialogs
{
    public partial class AlarmSettingsDialogViewModel : DialogViewModelBase<AlarmSettingsResult>
    {
        [ObservableProperty]
        private bool _isAlarmEnabled;

        [ObservableProperty]
        private double _alarmMinValue;

        [ObservableProperty]
        private double _alarmMaxValue;

        public AlarmSettingsDialogViewModel(bool currentIsAlarmEnabled, double currentAlarmMinValue, double currentAlarmMaxValue)
        {
            IsAlarmEnabled = currentIsAlarmEnabled;
            AlarmMinValue = currentAlarmMinValue;
            AlarmMaxValue = currentAlarmMaxValue;
            Title = "修改报警设置";
            PrimaryButText = "确定";
        }

        [RelayCommand]
        private void PrimaryButton()
        {
            var result = new AlarmSettingsResult
            {
                IsAlarmEnabled = IsAlarmEnabled,
                AlarmMinValue = AlarmMinValue,
                AlarmMaxValue = AlarmMaxValue
            };
            Close(result);
        }

        [RelayCommand]
        private void CancleButton()
        {
            Close(null);
        }
    }

    public class AlarmSettingsResult
    {
        public bool IsAlarmEnabled { get; set; }
        public double AlarmMinValue { get; set; }
        public double AlarmMaxValue { get; set; }
    }
}