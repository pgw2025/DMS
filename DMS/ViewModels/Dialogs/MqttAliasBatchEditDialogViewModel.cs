using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using DMS.Models;

namespace DMS.ViewModels.Dialogs;

public partial class MqttAliasBatchEditDialogViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "批量设置MQTT别名";

    [ObservableProperty]
    private ObservableCollection<VariableMqtt> _variablesToEdit;

    public Mqtt SelectedMqtt { get; private set; }

    public MqttAliasBatchEditDialogViewModel(List<Variable> selectedVariables, Mqtt selectedMqtt)
    {
        SelectedMqtt = selectedMqtt;
        Title=$"设置：{SelectedMqtt.Name}-MQTT服务器关联变量的别名";
        VariablesToEdit = new ObservableCollection<VariableMqtt>(
            selectedVariables.Select(v => new VariableMqtt(v, selectedMqtt))
        );
    }

    public MqttAliasBatchEditDialogViewModel()
    {
        // For design time
        VariablesToEdit = new ObservableCollection<VariableMqtt>();
    }
}