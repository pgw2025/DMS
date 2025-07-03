using PMSWPF.Models;

namespace PMSWPF.Services;

public interface IDialogService
{
    Task<Device> ShowAddDeviceDialog();
    Task<Device> ShowEditDeviceDialog(Device device);
    
    Task<bool> ShowConfrimeDialog(string title, string message,string buttonText="确认");
    
    Task<VariableTable> ShowAddVarTableDialog();
    
    Task<VariableData> ShowAddVarDataDialog();

    void ShowMessageDialog(string title, string message);
    Task<VariableData> ShowEditVarDataDialog(VariableData variableData);
}