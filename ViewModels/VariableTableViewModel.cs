using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using PMSWPF.Data.Repositories;
using PMSWPF.Enums;
using PMSWPF.Helper;
using PMSWPF.Models;

namespace PMSWPF.ViewModels;

partial class VariableTableViewModel : ViewModelBase
{
    // private readonly ILogger<VariableTableViewModel> _logger;
    [ObservableProperty] private VariableTable variableTable;
    [ObservableProperty] private ObservableCollection<VariableData> _dataVariables;

    /// <summary>
    /// 是否是第一次加载,防止ToggleSwitch第一次加载触发改变事件
    /// </summary>
    public bool IsLoadCompletion { get; set; } = false;

    private readonly VarTableRepository _varTableRepository;

    public VariableTableViewModel()
    {
        IsLoadCompletion = false;
        // _logger = logger;
        _varTableRepository = new VarTableRepository();
    }
   
    

    public override void OnLoaded()
    {
        
        if (VariableTable.DataVariables != null)
        {
            DataVariables = new ObservableCollection<VariableData>(VariableTable.DataVariables);
        }
        IsLoadCompletion = true;
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