using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.WPF.Models;
using DMS.WPF.Models;

namespace DMS.WPF.ViewModels.Dialogs;

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