using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class MqttDialogViewModel : DialogViewModelBase<MqttServerItemViewModel>
{
    [ObservableProperty]
    private MqttServerItemViewModel _mqttServer;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _primaryButText;

    public MqttDialogViewModel()
    {
        MqttServer = new MqttServerItemViewModel();
    }

    public MqttDialogViewModel(MqttServerItemViewModel mqttServer)
    {
        MqttServer = new MqttServerItemViewModel
        {
            Id = mqttServer.Id,
            ServerName = mqttServer.ServerName,
            ServerUrl = mqttServer.ServerUrl,
            Port = mqttServer.Port,
            Username = mqttServer.Username,
            Password = mqttServer.Password,
            IsActive = mqttServer.IsActive,
            SubscribeTopic = mqttServer.SubscribeTopic,
            PublishTopic = mqttServer.PublishTopic,
            ClientId = mqttServer.ClientId,
            CreatedAt = mqttServer.CreatedAt,
            ConnectedAt = mqttServer.ConnectedAt,
            ConnectionDuration = mqttServer.ConnectionDuration,
            MessageFormat = mqttServer.MessageFormat
        };
    }

    [RelayCommand]
    private void PrimaryButton()
    {
        
        Close(MqttServer);
    }

    [RelayCommand]
    private void CancelButton()
    {
        Close(null);
    }
}