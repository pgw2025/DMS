using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using DMS.WPF.ViewModels.Items;

namespace DMS.WPF.ViewModels.Dialogs;

public partial class MqttAliasBatchEditDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "批量设置MQTT别名";

    [ObservableProperty]
    private ObservableCollection<VariableMqttAlias> _variablesToEdit;

    public MqttServerItemViewModel SelectedMqtt { get; private set; }

    public MqttAliasBatchEditDialogViewModel(List<VariableItemViewModel> selectedVariables, MqttServerItemViewModel selectedMqtt)
    {
        SelectedMqtt = selectedMqtt;
        Title=$"设置：{SelectedMqtt.ServerName}-MQTT服务器关联变量的别名";
        // VariablesToEdit = new ObservableCollection<VariableMqttAlias>(
        //     selectedVariables.Select(v => new VariableMqttAlias(v, selectedMqtt))
        // );
    }

    public MqttAliasBatchEditDialogViewModel()
    {
        // For design time
        // VariablesToEdit = new ObservableCollection<VariableMqtt>();
    }
}