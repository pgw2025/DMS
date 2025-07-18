using DMS.Core.Enums;
using DMS.Models;
using iNKORE.UI.WPF.Modern.Controls;

namespace DMS.Services;

public interface IDialogService
{
    Task<Device> ShowAddDeviceDialog();
    Task<Device> ShowEditDeviceDialog(Device device);
    Task<Mqtt> ShowAddMqttDialog();
    Task<Mqtt> ShowEditMqttDialog(Mqtt mqtt);
    Task<bool> ShowConfrimeDialog(string title, string message,string buttonText="确认");
    
    Task<VariableTable> ShowAddVarTableDialog();
    Task<VariableTable> ShowEditVarTableDialog(VariableTable variableTable);
    
    Task<Variable> ShowAddVarDataDialog();

    void ShowMessageDialog(string title, string message);
    Task<Variable> ShowEditVarDataDialog(Variable variable);
    Task<string> ShowImportExcelDialog();
    ContentDialog ShowProcessingDialog(string title, string message);
    Task<PollLevelType?> ShowPollLevelDialog(PollLevelType pollLevelType);
    Task<Mqtt?> ShowMqttSelectionDialog();
    Task<List<Variable>> ShowOpcUaImportDialog(string endpointUrl);
    Task<OpcUaUpdateType?> ShowOpcUaUpdateTypeDialog();
    Task<bool?> ShowIsActiveDialog(bool currentIsActive);
    Task ShowImportResultDialog(List<string> importedVariables, List<string> existingVariables);
    Task<string?> ShowMqttAliasDialog(string variableName, string mqttServerName);
    Task<List<VariableMqtt>> ShowMqttAliasBatchEditDialog(List<Variable> selectedVariables, Mqtt selectedMqtt);
}