using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iNKORE.UI.WPF.Modern.Controls;
using Newtonsoft.Json;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.Services;

namespace PMSWPF.ViewModels;

partial class VariableTableViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private VariableTable variableTable;
    private ObservableCollection<VariableData> _dataVariables = new ObservableCollection<VariableData>();

    [ObservableProperty]
    private VariableData _selectedVariableData;

    [ObservableProperty]
    private string _searchText;

    public ICollectionView VariableDataView { get; private set; }

    /// <summary>
    /// 是否是第一次加载,防止ToggleSwitch第一次加载触发改变事件
    /// </summary>
    public bool IsLoadCompletion { get; set; } = false;

    private readonly VarTableRepository _varTableRepository;
    private readonly VarDataRepository _varDataRepository;
    private ObservableCollection<VariableData>? _originalDataVariables;


    [ObservableProperty]
    private bool _isS7ProtocolSelected;

    [ObservableProperty]
    private bool _isOpcUaProtocolSelected;

    public VariableTableViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        IsLoadCompletion = false;
        _varTableRepository = new VarTableRepository();
        _varDataRepository = new VarDataRepository();
        _dataVariables = new ObservableCollection<VariableData>(); // Initialize here
        VariableDataView = CollectionViewSource.GetDefaultView(_dataVariables);
        VariableDataView.Filter = FilterVariables;
    }

    private bool FilterVariables(object item)
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        if (item is VariableData variable)
        {
            var searchTextLower = SearchText.ToLower();
            return variable.Name?.ToLower().Contains(searchTextLower) == true ||
                   variable.Description?.ToLower().Contains(searchTextLower) == true ||
                   variable.NodeId?.ToLower().Contains(searchTextLower) == true ||
                   variable.S7Address?.ToLower().Contains(searchTextLower) == true ||
                   variable.DataValue?.ToLower().Contains(searchTextLower) == true ||
                   variable.DisplayValue?.ToLower().Contains(searchTextLower) == true;
        }
        return false;
    }

    partial void OnSearchTextChanged(string value)
    {
        VariableDataView?.Refresh();
    }



    public override void OnLoaded()
    {
        IsS7ProtocolSelected = VariableTable.ProtocolType == ProtocolType.S7;
        IsOpcUaProtocolSelected = VariableTable.ProtocolType == ProtocolType.OpcUA;

        if (VariableTable.DataVariables != null)
        {
            _dataVariables = new ObservableCollection<VariableData>(VariableTable.DataVariables);
            VariableDataView = CollectionViewSource.GetDefaultView(_dataVariables);
            VariableDataView.Filter = FilterVariables;

            // 3. 创建原始数据的深拷贝备份
            // 推荐使用 JSON 序列化/反序列化进行深度拷贝
            var serialized = JsonConvert.SerializeObject(_dataVariables);
            _originalDataVariables = JsonConvert.DeserializeObject<ObservableCollection<VariableData>>(serialized);

            // 在数据加载完成后，将所有变量的 IsModified 状态重置为 false
            foreach (var variableData in _dataVariables)
            {
                variableData.IsModified = false;
            }
        }


        IsLoadCompletion = true;
    }

    /// <summary>
    /// 退出当前实体时调用
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> OnExitAsync()
    {
        var modifiedDatas = _dataVariables.Where(d => d.IsModified == true)
                                         .ToList();
        if (modifiedDatas.Count == 0)
            return true;
        var isExit = await _dialogService.ShowConfrimeDialog(
            "数据未保存", $"你有{modifiedDatas.Count}个修改的变量没有保存，离开后这些数据就可能丢失了确认要离开吗？", "离开");
        if (!isExit)
        {
            // 不保存数据，还原原来的数据
            foreach (var modifiedData in modifiedDatas)
            {
                var oldData = _originalDataVariables.First(od => od.Id == modifiedData.Id);
                oldData.CopyTo(modifiedData);
                modifiedData.IsModified = false;
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// 保存修改过的变量数据
    /// </summary>
    [RelayCommand]
    private async void SaveModifiedVarData()
    {
        var modifiedDatas = _dataVariables.Where(d => d.IsModified == true)
                                         .ToList();
        ///更新数据库
        await _varDataRepository.UpdateAsync(modifiedDatas);
        // 还原修改状态
        foreach (var modifiedData in modifiedDatas)
        {
            modifiedData.IsModified = false;
        }

        NotificationHelper.ShowSuccess($"修改的{modifiedDatas.Count}变量保存成功.");
    }

    [RelayCommand]
    private async void EditVarData(VariableTable variableTable)
    {
        try
        {
            // // 1. 显示添加设备对话框
            var varData = await _dialogService.ShowEditVarDataDialog(SelectedVariableData);
            // // 如果用户取消或对话框未返回设备，则直接返回
            if (varData == null)
                return;
            varData.VariableTableId = variableTable.Id;
            // 更新数据库
            await _varDataRepository.UpdateAsync(varData);
            // 更新当前页面的
            var index = variableTable.DataVariables.IndexOf(SelectedVariableData);
            // 更新变量表中的
            if (index >= 0 && index < variableTable.DataVariables.Count)
                variableTable.DataVariables[index] = varData;
            NotificationHelper.ShowSuccess($"编辑变量成功:{varData?.Name}");
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"编辑变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }

    [RelayCommand]
    private async void ImprotFromTiaVarTable()
    {
        ContentDialog processingDialog = null;
        try
        {
            // 让用户选择导入的Excel文件
            var filePath = await _dialogService.ShowImportExcelDialog();
            if (string.IsNullOrEmpty(filePath))
                return;
            // 读取Excel转换成VariableData列表
            var importVarDataList = ExcelHelper.ImprotFromTiaVariableTable(filePath);
            if (importVarDataList.Count == 0)
                return;
           processingDialog= _dialogService.ShowProcessingDialog("正在处理...", "正在导入变量,请稍等片刻....");
            
            foreach (var variableData in importVarDataList)
            {
                variableData.CreateTime=DateTime.Now;
                variableData.VariableTableId = VariableTable.Id;
            }
            // 插入数据库
            var resVarDataCount= await _varDataRepository.AddAsync(importVarDataList);
            //更新界面
            // variableTable.DataVariables.AddRange(resVarDataList);
            variableTable.DataVariables= await _varDataRepository.GetAllAsync();
            _dataVariables=new ObservableCollection<VariableData>(variableTable.DataVariables);
            processingDialog?.Hide();
            
            string msgSuccess = $"成功导入变量：{resVarDataCount}个。";
            NlogHelper.Info(msgSuccess);
            NotificationHelper.ShowSuccess(msgSuccess);
            
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"从TIA导入变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
        
        
    }

    [RelayCommand]
    private async Task ImportFromOpcUaServer()
    {
        try
        {
            string opcUaEndpointUrl = VariableTable?.Device?.OpcUaEndpointUrl;
            if (string.IsNullOrEmpty(opcUaEndpointUrl))
            {
                NotificationHelper.ShowError("OPC UA Endpoint URL 未设置。请在设备详情中配置。");
                return;
            }
            var importedVariables = await _dialogService.ShowOpcUaImportDialog(opcUaEndpointUrl);
            if (importedVariables == null || !importedVariables.Any())
            {
                return; // 用户取消或没有选择任何变量
            }

            ContentDialog processingDialog = _dialogService.ShowProcessingDialog("正在处理...", "正在导入OPC UA变量,请稍等片刻....");

            foreach (var variableData in importedVariables)
            {
                variableData.CreateTime = DateTime.Now;
                variableData.VariableTableId = VariableTable.Id;
                variableData.ProtocolType = ProtocolType.OpcUA; // 确保协议类型正确
            }

            // 插入数据库
            var resVarDataCount = await _varDataRepository.AddAsync(importedVariables);

            // 更新界面
            variableTable.DataVariables = await _varDataRepository.GetAllAsync();
            _dataVariables = new ObservableCollection<VariableData>(variableTable.DataVariables);
            processingDialog?.Hide();

            string msgSuccess = $"成功导入OPC UA变量：{resVarDataCount}个。";
            NlogHelper.Info(msgSuccess);
            NotificationHelper.ShowSuccess(msgSuccess);
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"从OPC UA服务器导入变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }


    [RelayCommand]
    private async void AddVarData(VariableTable variableTable)
    {
        try
        {
            // // 1. 显示添加设备对话框
            var varData = await _dialogService.ShowAddVarDataDialog();
            // // 如果用户取消或对话框未返回设备，则直接返回
            if (varData == null)
                return;

            varData.VariableTableId = variableTable.Id;
            // 更新数据库
            await _varDataRepository.AddAsync(varData);
            // 更新当前页面的
            _dataVariables.Add(varData);
            NotificationHelper.ShowSuccess($"添加变量成功:{varData?.Name}");
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"添加变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }

    [RelayCommand]
    public async Task DeleteVarData(List<VariableData> variablesToDelete)
    {
        if (variablesToDelete == null || !variablesToDelete.Any())
        {
            NotificationHelper.ShowInfo("请选择要删除的变量");
            return;
        }

        var names = string.Join("、", variablesToDelete.Select(v => v.Name));
        var confirm = await _dialogService.ShowConfrimeDialog(
            "删除确认",
            $"确定要删除选中的 {variablesToDelete.Count} 个变量吗？\n\n{names}",
            "删除");

        if (!confirm)
            return;

        try
        {
            var result = await _varDataRepository.DeleteAsync(variablesToDelete);
            if (result > 0)
            {
                foreach (var variable in variablesToDelete)
                {
                    _dataVariables.Remove(variable);
                }
                NotificationHelper.ShowSuccess($"成功删除 {result} 个变量");
            }
            else
            {
                NotificationHelper.ShowError("删除变量失败");
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"删除变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }

    [RelayCommand]
    public async Task ChangePollLevel(List<VariableData> variablesToChange)
    {
        if (variablesToChange == null || !variablesToChange.Any())
        {
            NotificationHelper.ShowInfo("请选择要修改轮询频率的变量");
            return;
        }

        var newPollLevelType = await _dialogService.ShowPollLevelDialog(variablesToChange.First().PollLevelType);
        if (newPollLevelType.HasValue)
        {
            foreach (var variable in variablesToChange)
            {
                variable.PollLevelType = newPollLevelType.Value;
                variable.IsModified=false;
            }

            await _varDataRepository.UpdateAsync(variablesToChange);
            NotificationHelper.ShowSuccess($"已成功更新 {variablesToChange.Count} 个变量的轮询频率");
        }
    }

    [RelayCommand]
    public async Task AddMqttServerToVariables(IList<VariableData> variablesToAddMqtt)
    {
        if (variablesToAddMqtt == null || !variablesToAddMqtt.Any())
        {
            NotificationHelper.ShowInfo("请选择要添加MQTT服务器的变量");
            return;
        }

        try
        {
            var selectedMqtt = await _dialogService.ShowMqttSelectionDialog();
            if (selectedMqtt == null)
            {
                return; // 用户取消选择
            }

            foreach (VariableData variable in variablesToAddMqtt)
            {
                if (variable.Mqtts == null)
                {
                    variable.Mqtts = new List<Mqtt>();
                }
                // 避免重复添加
                if (!variable.Mqtts.Any(m => m.Id == selectedMqtt.Id))
                {
                    variable.Mqtts.Add(selectedMqtt);
                    // variable.IsModified = true; // 标记为已修改
                }
            }

            // 批量更新数据库
            await _varDataRepository.UpdateAsync(variablesToAddMqtt.ToList());
            NotificationHelper.ShowSuccess($"已成功为 {variablesToAddMqtt.Count} 个变量添加MQTT服务器: {selectedMqtt.Name}");
        }
        catch (Exception ex)
        {
            NotificationHelper.ShowError($"添加MQTT服务器失败: {ex.Message}", ex);
        }
    }

    // [RelayCommand]
    // private async void ImportFromExcel()
    // {
    //     var filePath = await _dialogService.ShowImportExcelDialog();
    //     if (!string.IsNullOrEmpty(filePath))
    //     {
    //         // TODO: Implement Excel import logic using the filePath
    //         NotificationHelper.ShowMessage($"Successfully imported from {filePath}", NotificationType.Success);
    //     }
    // }


    public async Task OnIsActiveChanged(bool active)
    {
        var res = await _varTableRepository.Edit(VariableTable);
        if (res > 0)
        {
            var statusMessage = active ? "已启用" : "已停用";
            NotificationHelper.ShowSuccess($"变量表：{VariableTable.Name},{statusMessage}");
        }
        else
        {
            NotificationHelper.ShowError($"变量表：{VariableTable.Name},状态修改失败，状态：{active}");
            // _logger.LogInformation($"变量表：{VariableTable.Name},状态修改失败，状态：{active}");
        }
    }
}