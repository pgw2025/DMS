using iNKORE.UI.WPF.Modern.Controls;
using NPOI.SS.Formula.Functions;
using PMSWPF.Models;
using PMSWPF.ViewModels.Dialogs;
using PMSWPF.Views.Dialogs;

namespace PMSWPF.Services;

public class DialogService :IDialogService
{
    public DialogService()
    {

    }

    public async Task<Device> ShowAddDeviceDialog()
    {
        var device = new Device();
        DeviceDialogViewModel vm = new DeviceDialogViewModel(device);
        vm.Title = "添加设备";
        vm.PrimaryButContent = "添加设备";
        return await ShowConentDialog(vm,device);
    }

    private static async Task<Device> ShowConentDialog(DeviceDialogViewModel viewModel,Device device)
    {
        var dialog = new DeviceDialog(viewModel);
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            return device;
        }
        return null;
    }

    public async Task<Device> ShowEditDeviceDialog(Device device)
    {
        DeviceDialogViewModel vm = new DeviceDialogViewModel(device);
        vm.Title = "编辑设备";
        vm.PrimaryButContent = "编辑设备";
        return await ShowConentDialog(vm,device);
        
    }

    public async Task<bool> ShowConfrimeDialog(string title, string message,string buttonText="确认")
    {
        ConfrimDialogViewModel vm = new ConfrimDialogViewModel();
        vm.Title = title;
        vm.Message = message;
        vm.PrimaryButtonText = buttonText;
        var dialog = new ConfirmDialog(vm);
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            return true;
        }
        return false;
    }

    public async Task<VariableTable> ShowAddVarTableDialog()
    {
        VarTableDialogViewModel vm = new();
        vm.Title = "添加变量表";
        vm.PrimaryButtonText =  "添加变量表";
        vm.VariableTable = new VariableTable();
        var dialog = new VarTableDialog(vm);
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            return vm.VariableTable;
        }
        return null;
    }
    
    public async Task<VariableData> ShowAddVarDataDialog()
    {
        VarDataDialogViewModel vm = new();
        vm.Title = "添加变量";
        vm.PrimaryButtonText =  "添加变量";
        vm.VariableData = new VariableData();
        var dialog = new VarDataDialog(vm);
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            return vm.VariableData;
        }
        return null;
    }


    public void ShowMessageDialog(string title, string message)
    {
        MessageBox.Show(message);
    }

    public async Task<VariableData> ShowEditVarDataDialog(VariableData variableData)
    {
        VarDataDialogViewModel vm = new();
        vm.Title = "编辑变量";
        vm.PrimaryButtonText =  "编辑变量";
        vm.VariableData = variableData;
        var dialog = new VarDataDialog(vm);
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            return vm.VariableData;
        }
        return null;
    }
}