using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using iNKORE.UI.WPF.Modern.Controls;
using Newtonsoft.Json;
using NLog;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Models;
using PMSWPF.Services;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace PMSWPF.ViewModels;

partial class VariableTableViewModel : ViewModelBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private readonly IDialogService _dialogService;

    // private readonly ILogger<VariableTableViewModel> _logger;
    [ObservableProperty]
    private VariableTable variableTable;

    [ObservableProperty]
    private ObservableCollection<VariableData> _dataVariables;

    private ObservableCollection<VariableData> _originalDataVariables;

    [ObservableProperty]
    private VariableData _selectedVariableData;

    /// <summary>
    /// 是否是第一次加载,防止ToggleSwitch第一次加载触发改变事件
    /// </summary>
    public bool IsLoadCompletion { get; set; } = false;

    private readonly VarTableRepository _varTableRepository;
    private readonly VarDataRepository _varDataRepository;


    public VariableTableViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        IsLoadCompletion = false;
        // _logger = logger;
        _varTableRepository = new VarTableRepository();
        _varDataRepository = new VarDataRepository();
    }


    public override void OnLoaded()
    {
        if (VariableTable.DataVariables != null)
        {
            DataVariables = new ObservableCollection<VariableData>(VariableTable.DataVariables);
            // 3. 创建原始数据的深拷贝备份
            // 推荐使用 JSON 序列化/反序列化进行深度拷贝
            var serialized = JsonConvert.SerializeObject(DataVariables);
            _originalDataVariables = JsonConvert.DeserializeObject<ObservableCollection<VariableData>>(serialized);
        }


        IsLoadCompletion = true;
    }

    /// <summary>
    /// 退出当前实体时调用
    /// </summary>
    /// <returns></returns>
    public override async Task<bool> OnExitAsync()
    {
        var modifiedDatas = DataVariables.Where(d => d.IsModified == true)
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
        var modifiedDatas = DataVariables.Where(d => d.IsModified == true)
                                         .ToList();
        ///更新数据库
        await _varDataRepository.UpdateAsync(modifiedDatas);
        // 还原修改状态
        foreach (var modifiedData in modifiedDatas)
        {
            modifiedData.IsModified = false;
        }

        NotificationHelper.ShowMessage($"修改的{modifiedDatas.Count}变量保存成功.", NotificationType.Success);
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
            NotificationHelper.ShowMessage($"编辑变量成功:{varData?.Name}", NotificationType.Success);
        }
        catch (Exception e)
        {
            string msg = $"编辑变量的过程中发生了不可预期的错误：";
            Logger.Error(msg + e);
            NotificationHelper.ShowMessage(msg + e.Message, NotificationType.Error);
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
            DataVariables=new ObservableCollection<VariableData>(variableTable.DataVariables);
            processingDialog?.Hide();
            
            string msgSuccess = $"成功导入变量：{resVarDataCount}个。";
            Logger.Info(msgSuccess);
            NotificationHelper.ShowMessage(msgSuccess, NotificationType.Success);
            
        }
        catch (Exception e)
        {
            string msg = $"从TIA导入变量的过程中发生了不可预期的错误：";
            Logger.Error(msg + e);
            NotificationHelper.ShowMessage(msg + e.Message, NotificationType.Error);
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
            DataVariables.Add(varData);
            NotificationHelper.ShowMessage($"添加变量成功:{varData?.Name}", NotificationType.Success);
        }
        catch (Exception e)
        {
            string msg = $"添加变量的过程中发生了不可预期的错误：";
            Logger.Error(msg + e);
            NotificationHelper.ShowMessage(msg + e.Message, NotificationType.Error);
        }
    }

    [RelayCommand]
    private async void DeleteVarData()
    {
        if (SelectedVariableData == null)
        {
            NotificationHelper.ShowMessage("请选择要删除的变量", NotificationType.Warning);
            return;
        }

        var confirm = await _dialogService.ShowConfrimeDialog(
            "删除确认",
            $"确定要删除变量 \"{SelectedVariableData.Name}\" 吗？",
            "删除");

        if (!confirm)
            return;

        try
        {
            var result = await _varDataRepository.DeleteAsync(SelectedVariableData);
            if (result > 0)
            {
                var dataName = SelectedVariableData.Name;
                DataVariables.Remove(SelectedVariableData);
                NotificationHelper.ShowMessage($"变量 \"{dataName}\" 删除成功", NotificationType.Success);
            }
            else
            {
                NotificationHelper.ShowMessage($"变量 \"{SelectedVariableData.Name}\" 删除失败", NotificationType.Error);
            }
        }
        catch (Exception e)
        {
            string msg = $"删除变量的过程中发生了不可预期的错误：";
            Logger.Error(msg + e);
            NotificationHelper.ShowMessage(msg + e.Message, NotificationType.Error);
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
            NotificationHelper.ShowMessage($"变量表：{VariableTable.Name},{statusMessage}", NotificationType.Success);
        }
        else
        {
            NotificationHelper.ShowMessage($"变量表：{VariableTable.Name},状态修改失败，状态：{active}", NotificationType.Error);
            // _logger.LogInformation($"变量表：{VariableTable.Name},状态修改失败，状态：{active}");
        }
    }
}