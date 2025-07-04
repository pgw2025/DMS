using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PMSWPF.Models;

namespace PMSWPF.ViewModels.Dialogs;

public partial class MqttDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private Mqtt _mqtt;
    
    [ObservableProperty] private string title ;
    [ObservableProperty] private string primaryButContent ;

    public MqttDialogViewModel(Mqtt mqtt)
    {
        _mqtt = mqtt;
    }


    [RelayCommand]
    public void AddMqtt()
    {

    }
}