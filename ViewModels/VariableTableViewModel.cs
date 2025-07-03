using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
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
        }


        IsLoadCompletion = true;
    }

    [RelayCommand]
    public async void EditVarData(VariableTable variableTable)
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
    public async void AddVarData(VariableTable variableTable)
    {
        try
        {
            // // 1. 显示添加设备对话框
            var varData = await _dialogService.ShowAddVarDataDialog();
            // // 如果用户取消或对话框未返回设备，则直接返回
            if (varData == null)
                return;

            varData.VariableTableId = variableTable.Id;
            var addVarData = await _varDataRepository.AddAsync(varData);
            DataVariables?.Add(addVarData);
            variableTable.DataVariables?.Add(addVarData);
            var msg = addVarData.Id > 0 ? $"添加变量成功:{varData?.Name}" : $"添加变量成功:{varData.Name}";
            var type = addVarData.Id > 0 ? NotificationType.Success : NotificationType.Error;
            NotificationHelper.ShowMessage(msg, type);
        }
        catch (Exception e)
        {
            string msg = $"添加变量的过程中发生了不可预期的错误：";
            Logger.Error(msg + e);
            NotificationHelper.ShowMessage(msg + e.Message, NotificationType.Error);
        }
    }


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