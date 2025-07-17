using iNKORE.UI.WPF.Modern.Controls;
using PMSWPF.Enums;
using PMSWPF.Models;

namespace PMSWPF.Services;

public interface IDialogService
{
    Task<Device> ShowAddDeviceDialog();
    Task<Device> ShowEditDeviceDialog(Device device);
    Task<Mqtt> ShowAddMqttDialog();
    Task<Mqtt> ShowEditMqttDialog(Mqtt mqtt);
    Task<bool> ShowConfrimeDialog(string title, string message,string buttonText="确认");
    
    Task<VariableTable> ShowAddVarTableDialog();
    Task<VariableTable> ShowEditVarTableDialog(VariableTable variableTable);
    
    Task<VariableData> ShowAddVarDataDialog();

    void ShowMessageDialog(string title, string message);
    Task<VariableData> ShowEditVarDataDialog(VariableData variableData);
    Task<string> ShowImportExcelDialog();
    ContentDialog ShowProcessingDialog(string title, string message);
    Task<PollLevelType?> ShowPollLevelDialog(PollLevelType pollLevelType);
    Task<Mqtt?> ShowMqttSelectionDialog();
    Task<List<VariableData>> ShowOpcUaImportDialog(string endpointUrl);
    Task<OpcUaUpdateType?> ShowOpcUaUpdateTypeDialog();
    Task<bool?> ShowIsActiveDialog(bool currentIsActive);
    Task ShowImportResultDialog(List<string> importedVariables, List<string> existingVariables);
}