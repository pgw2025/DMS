using DMS.Enums;
using DMS.Models;
using DMS.ViewModels.Dialogs;
using DMS.Views.Dialogs;
using HandyControl.Tools.Extension;
using iNKORE.UI.WPF.Modern.Controls;
using NPOI.SS.Formula.Functions;
using DMS.Data.Repositories;

namespace DMS.Services;

public class DialogService :IDialogService
{
    private readonly DataServices _dataServices;

    public DialogService(DataServices dataServices)
    {
        _dataServices = dataServices;
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

    public async Task<Mqtt> ShowAddMqttDialog()
    {
        var mqtt = new Mqtt();
        MqttDialogViewModel vm = new MqttDialogViewModel(mqtt);
        vm.Title = "添加MQTT";
        vm.PrimaryButContent = "添加MQTT";
        return await ShowConentDialog(vm, mqtt);
    }

    public async Task<Mqtt> ShowEditMqttDialog(Mqtt mqtt)
    {
        MqttDialogViewModel vm = new MqttDialogViewModel(mqtt);
        vm.Title = "编辑MQTT";
        vm.PrimaryButContent = "编辑MQTT";
        return await ShowConentDialog(vm, mqtt);
    }

    private static async Task<Mqtt> ShowConentDialog(MqttDialogViewModel viewModel, Mqtt mqtt)
    {
        var dialog = new MqttDialog(viewModel);
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            return mqtt;
        }
        return null;
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

    public async Task<VariableTable> ShowEditVarTableDialog(VariableTable variableTable)
    {
        VarTableDialogViewModel vm = new();
        vm.Title = "编辑变量表";
        vm.PrimaryButtonText = "编辑变量表";
        vm.VariableTable = variableTable;
        var dialog = new VarTableDialog(vm);
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            return vm.VariableTable;
        }
        return null;
    }
    
    public async Task<Variable> ShowAddVarDataDialog()
    {
        VarDataDialogViewModel vm = new();
        vm.Title = "添加变量";
        vm.PrimaryButtonText =  "添加变量";
        vm.Variable = new Variable();
        var dialog = new VarDataDialog(vm);
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            return vm.Variable;
        }
        return null;
    }


    public void ShowMessageDialog(string title, string message)
    {
        MessageBox.Show(message);
    }

    public async Task<Variable> ShowEditVarDataDialog(Variable variable)
    {
        VarDataDialogViewModel vm = new();
        vm.Title = "编辑变量";
        vm.PrimaryButtonText =  "编辑变量";
        vm.Variable = variable;
        var dialog = new VarDataDialog(vm);
        var res = await dialog.ShowAsync();
        if (res == ContentDialogResult.Primary)
        {
            return vm.Variable;
        }
        return null;
    }

    public async Task<string> ShowImportExcelDialog()
    {
        var vm = new ImportExcelDialogViewModel();
        var dialog = new ImportExcelDialog(vm);
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return vm.FilePath;
        }
        return null;
    }

    public ContentDialog ShowProcessingDialog(string title, string message)
    {
        ProcessingDialogViewModel vm = new ProcessingDialogViewModel();
        vm.Title = title;
        vm.Message = message;
        var dialog = new ProcessingDialog(vm);
        _ = dialog.ShowAsync(); // 不await，让它在后台显示
        return dialog;
    }

    public async Task<PollLevelType?> ShowPollLevelDialog(PollLevelType pollLevelType)
    {
        var vm = new PollLevelDialogViewModel(pollLevelType);
        var dialog = new PollLevelDialog(vm);
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return vm.SelectedPollLevelType;
        }
        return null;
    }

    public async Task<Mqtt?> ShowMqttSelectionDialog()
    {
        var vm = new MqttSelectionDialogViewModel(_dataServices);
        var dialog = new MqttSelectionDialog(vm);
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? vm.SelectedMqtt : null;
    }

    public async Task<List<Variable>> ShowOpcUaImportDialog(string endpointUrl)
    {
       var vm= new OpcUaImportDialogViewModel();
       vm.EndpointUrl = endpointUrl;
        var dialog = new OpcUaImportDialog(vm);
        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary ? vm.GetSelectedVariables().ToList() : null;
    }

    public async Task<OpcUaUpdateType?> ShowOpcUaUpdateTypeDialog()
    {
        var vm = new OpcUaUpdateTypeDialogViewModel();
        var dialog = new OpcUaUpdateTypeDialog(vm);
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return vm.SelectedUpdateType;
        }
        return null;
    }

    public async Task<bool?> ShowIsActiveDialog(bool currentIsActive)
    {
        var vm = new IsActiveDialogViewModel(currentIsActive);
        var dialog = new IsActiveDialog(vm);
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return vm.SelectedIsActive;
        }
        return null;
    }

    public async Task ShowImportResultDialog(List<string> importedVariables, List<string> existingVariables)
    {
        var vm = new ImportResultDialogViewModel(importedVariables, existingVariables);
        var dialog = new ImportResultDialog(vm);
        await dialog.ShowAsync();
    }

    public async Task<string?> ShowMqttAliasDialog(string variableName, string mqttServerName)
    {
        var vm = new MqttAliasDialogViewModel(variableName, mqttServerName);
        var dialog = new MqttAliasDialog(vm);
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return vm.MqttAlias;
        }
        return null;
    }

    public async Task<List<VariableMqtt>> ShowMqttAliasBatchEditDialog(List<Variable> selectedVariables, Mqtt selectedMqtt)
    {
        var vm = new MqttAliasBatchEditDialogViewModel(selectedVariables, selectedMqtt);
        var dialog = new MqttAliasBatchEditDialog(vm);
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            return vm.VariablesToEdit.ToList();
        }
        return null;
    }
}