using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class MqttAliasDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "设置MQTT别名";

    [ObservableProperty]
    private string _message = "请输入变量在该MQTT服务器上的别名：";

    [ObservableProperty]
    private string _variableName = string.Empty;

    [ObservableProperty]
    private string _mqttServerName = string.Empty;

    [ObservableProperty]
    private string _mqttAlias = string.Empty;

    public MqttAliasDialogViewModel(string variableName, string mqttServerName)
    {
        VariableName = variableName;
        MqttServerName = mqttServerName;
        Message = $"请输入变量 '{VariableName}' 在MQTT服务器 '{MqttServerName}' 上的别名：";
    }

    public MqttAliasDialogViewModel()
    {
        
    }
}