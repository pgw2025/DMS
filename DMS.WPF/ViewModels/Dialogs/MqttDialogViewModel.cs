using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class MqttDialogViewModel : DialogViewModelBase<MqttServerItem>
{
    [ObservableProperty]
    private MqttServerItem _mqttServer;

    [ObservableProperty]
    private string _title;

    [ObservableProperty]
    private string _primaryButText;

    public MqttDialogViewModel()
    {
        MqttServer = new MqttServerItem();
    }

    public MqttDialogViewModel(MqttServerItem mqttServer)
    {
        MqttServer = new MqttServerItem
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
            MessageFormat = mqttServer.MessageFormat,
            MessageHeader = mqttServer.MessageHeader,
            MessageContent = mqttServer.MessageContent,
            MessageFooter = mqttServer.MessageFooter
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