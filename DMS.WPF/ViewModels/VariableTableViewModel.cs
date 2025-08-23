using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Core.Enums;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ViewModels.Items;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Services;
using DMS.Helper;
using DMS.WPF.Views;
using DMS.WPF.Views.HandyDialogs;
using HandyControl.Controls;
using HandyControl.Data;
using HandyControl.Tools.Extension;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;

namespace DMS.WPF.ViewModels;

partial class VariableTableViewModel : ViewModelBase, INavigatable
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 对话服务接口，用于显示各种对话框（如确认、编辑、导入等）。
    /// </summary>
    private readonly IDialogService _dialogService;

    private readonly IVariableAppService _variableAppService;

    /// <summary>
    /// 当前正在操作的变量表实体。
    /// 通过 ObservableProperty 自动生成 VariableTable 属性和 OnVariableTableChanged 方法。
    /// </summary>
    [ObservableProperty]
    private VariableTableItemViewModel currentVariableTable;

    /// <summary>
    /// 存储当前变量表中的所有变量数据的集合。
    /// 通过 ObservableProperty 自动生成 Variables 属性和 OnVariablesChanged 方法。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<VariableItemViewModel> _variables;

    /// <summary>
    /// 当前选中的变量数据。
    /// 通过 ObservableProperty 自动生成 SelectedVariable 属性和 OnSelectedVariableDataChanged 方法。
    /// </summary>
    [ObservableProperty]
    private VariableItemViewModel _selectedVariable;

    /// <summary>
    /// 用于过滤变量数据的搜索文本。
    /// 通过 ObservableProperty 自动生成 SearchText 属性和 OnSearchTextChanged 方法。
    /// </summary>
    [ObservableProperty]
    private string _searchText;


    /// <summary>
    /// 指示视图是否已完成首次加载。
    /// 用于防止某些UI控件（如ToggleSwitch）在初始化时触发不必要的事件。
    /// </summary>
    public bool IsLoadCompletion { get; set; } = false;


    /// <summary>
    /// 原始变量数据的深拷贝备份，用于在用户取消保存时还原数据。
    /// </summary>
    private ObservableCollection<VariableItemViewModel>? _originalVariables;

    /// <summary>
    /// 指示当前变量表是否使用S7协议。
    /// 通过 ObservableProperty 自动生成 IsS7ProtocolSelected 属性。
    /// </summary>
    [ObservableProperty]
    private bool _isS7ProtocolSelected;

    /// <summary>
    /// 指示当前变量表是否使用OpcUA协议。
    /// 通过 ObservableProperty 自动生成 IsOpcUaProtocolSelected 属性。
    /// </summary>
    [ObservableProperty]
    private bool _isOpcUaProtocolSelected;

    /// <summary>
    /// VariableTableViewModel 的构造函数。
    /// 初始化服务、数据仓库和变量数据集合视图。
    /// </summary>
    /// <param name="dialogService">对话服务接口的实例。</param>
    private readonly DataServices _dataServices;

    private readonly ObservableList<VariableItemViewModel> _variableItemList;
    private readonly ISynchronizedView<VariableItemViewModel, VariableItemViewModel> _synchronizedView;
    public NotifyCollectionChangedSynchronizedViewList<VariableItemViewModel> VariableItemListView { get; }

    public VariableTableViewModel(IMapper mapper, IDialogService dialogService, IVariableAppService variableAppService,
                                  DataServices dataServices)
    {
        _mapper = mapper;
        _dialogService = dialogService;
        _variableAppService = variableAppService;
        _dataServices = dataServices;
        IsLoadCompletion = false; // 初始设置为 false，表示未完成加载
        _variables = new ObservableCollection<VariableItemViewModel>(); // 初始化集合


        _variableItemList = new ObservableList<VariableItemViewModel>();
        _synchronizedView = _variableItemList.CreateView(v => v);

        VariableItemListView = _synchronizedView.ToNotifyCollectionChanged();
    }

    /// <summary>
    /// 用于过滤 <see cref="VariableView"/> 中的变量数据。
    /// 根据 <see cref="SearchText"/> 属性的值进行模糊匹配。
    /// </summary>
    /// <param name="item">要过滤的集合中的单个项。</param>
    /// <returns>如果项匹配搜索条件则为 true，否则为 false。</returns>
    private bool FilterVariables(VariableItemViewModel item)
    {
        // 尝试将项转换为 Variable 类型
        var searchTextLower = SearchText.ToLower();
        // 检查变量的名称、描述、NodeId、S7地址、数据值或显示值是否包含搜索文本
        return item.Name?.ToLower()
                   .Contains(searchTextLower) == true ||
               item.Description?.ToLower()
                   .Contains(searchTextLower) == true ||
               item.OpcUaNodeId?.ToLower()
                   .Contains(searchTextLower) == true ||
               item.S7Address?.ToLower()
                   .Contains(searchTextLower) == true ||
               item.DataValue?.ToLower()
                   .Contains(searchTextLower) == true ||
               item.DisplayValue?.ToLower()
                   .Contains(searchTextLower) == true;
    }

    /// <summary>
    /// 当 <see cref="SearchText"/> 属性的值发生改变时自动调用。
    /// 刷新 <see cref="VariableView"/> 以应用新的过滤条件。
    /// </summary>
    /// <param name="value">新的搜索文本值。</param>
    partial void OnSearchTextChanged(string value)
    {
        // 如果搜索文本为空或空白，则显示所有项
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            _synchronizedView.ResetFilter();
        }
        else
        {
            _synchronizedView.AttachFilter(FilterVariables);
        }
    }

    /// <summary>
    /// 当视图加载完成时调用。
    /// 初始化协议选择状态，加载变量数据，并创建原始数据的深拷贝备份。
    /// </summary>
    public override void OnLoaded()
    {
        _variableItemList.AddRange(CurrentVariableTable.Variables);
        //
        //     // 创建原始数据的深拷贝备份，用于在取消保存时还原
        //     var settings = new JsonSerializerSettings
        //     {
        //         ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //     };
        //     var serialized = JsonConvert.SerializeObject(Variables, settings);
        //     _originalVariables = JsonConvert.DeserializeObject<ObservableCollection<Variable>>(serialized);
        //
        //     // 在数据加载完成后，将所有变量的 IsModified 状态重置为 false
        //     foreach (var variable in Variables)
        //     {
        //         variable.IsModified = false;
        //     }

        // 标记加载完成
        IsLoadCompletion = true;
    }

    /// <summary>
    /// 当用户尝试退出当前视图时异步调用。
    /// 检查是否有未保存的修改，并提示用户是否保存或放弃更改。
    /// </summary>
    /// <returns>如果可以安全退出则为 true，否则为 false。</returns>
    public override async Task<bool> OnExitAsync()
    {
        // 查找所有已修改的变量数据
        var modifiedDatas = Variables.Where(d => d.IsModified == true)
                                     .ToList();
        // 如果没有修改，则直接允许退出
        if (modifiedDatas.Count == 0)
            return true;

        // 提示用户有未保存的数据，询问是否离开
        // var isExit = await _dialogService.ShowConfrimeDialog(
        //     "数据未保存", $"你有{modifiedDatas.Count}个修改的变量没有保存，离开后这些数据就可能丢失了确认要离开吗？", "离开");

        // // 如果用户选择不离开（即不保存），则还原数据
        // if (!isExit)
        // {
        //     // 遍历所有已修改的数据，从原始备份中还原
        //     foreach (var modifiedData in modifiedDatas)
        //     {
        //         var oldData = _originalVariables.First(od => od.Id == modifiedData.Id);
        //         // 将原始数据复制回当前数据
        //         _mapper.Map(oldData, modifiedData);
        //         modifiedData.IsModified = false; // 重置修改状态
        //     }
        //
        //     return false; // 不允许退出
        // }

        return true; // 允许退出
    }

    /// <summary>
    /// 保存所有已修改的变量数据到数据库。
    /// 此命令通常绑定到UI中的“保存”按钮。
    /// </summary>
    [RelayCommand]
    private async void SaveModifiedVarData()
    {
        // // 查找所有已标记为修改的变量数据
        // var modifiedDatas = Variables.Where(d => d.IsModified == true)
        //                                  .ToList();
        // // 更新数据库中的这些数据
        // await _varDataRepository.UpdateAsync(modifiedDatas);
        //
        // // 还原所有已保存数据的修改状态
        // foreach (var modifiedData in modifiedDatas)
        // {
        //     modifiedData.IsModified = false;
        // }
        //
        // // 显示成功通知
        // NotificationHelper.ShowSuccess($"修改的{modifiedDatas.Count}变量保存成功.");
    }

    /// <summary>
    /// 编辑选定的变量数据。
    /// 此命令通常绑定到UI中的“编辑”按钮或双击事件。
    /// </summary>
    /// <param name="variableTable">当前操作的变量表，用于更新其内部的变量数据。</param>
    [RelayCommand]
    private async void UpdateVariable(VariableTable variableTable)
    {
        // try
        // {
        //     // 显示编辑变量数据的对话框，并传入当前选中的变量数据
        //     var varData = await _dialogService.ShowEditVarDataDialog(SelectedVariable);
        //
        //     // 如果用户取消或对话框未返回数据，则直接返回
        //     if (varData == null)
        //         return;
        //
        //     // 设置变量数据的所属变量表ID
        //     varData.VariableTableId = variableTable.Id;
        //
        //     // 更新数据库中的变量数据
        //     await _varDataRepository.UpdateAsync(varData);
        //
        //     // 更新当前页面显示的数据：找到原数据在集合中的索引并替换
        //     var index = variableTable.Variables.IndexOf(SelectedVariable);
        //     if (index >= 0 && index < variableTable.Variables.Count)
        //         variableTable.Variables[index] = varData; // 替换为编辑后的数据
        //
        //     // 显示成功通知
        //     NotificationHelper.ShowSuccess($"编辑变量成功:{varData?.Name}");
        // }
        // catch (Exception e)
        // {
        //     // 捕获并显示错误通知
        //     NotificationHelper.ShowError($"编辑变量的过程中发生了不可预期的错误：{e.Message}", e);
        // }
    }

    /// <summary>
    /// 从TIA Portal导出的变量表Excel文件中导入变量数据。
    /// 此命令通常绑定到UI中的“从TIA导入”按钮。
    /// </summary>
    [RelayCommand]
    private async void ImprotFromTiaVarTable()
    {
        try
        {
            ImportExcelDialogViewModel
                viewModel = App.Current.Services.GetRequiredService<ImportExcelDialogViewModel>();
            List<Variable> improtVariable = await _dialogService.ShowDialogAsync(viewModel);

            if (improtVariable == null || improtVariable.Count == 0) return;

            var improtVariableDtos = _mapper.Map<List<VariableDto>>(improtVariable);
            foreach (var variableDto in improtVariableDtos)
            {
                variableDto.CreatedAt = DateTime.Now;
                variableDto.UpdatedAt = DateTime.Now;
                variableDto.VariableTableId = CurrentVariableTable.Id;
            }

            var existList = await _variableAppService.FindExistingVariablesAsync(improtVariableDtos);
            if (existList.Count > 0)
            {
                // // 拼接要删除的变量名称，用于确认提示
                var existNames = string.Join("、", existList.Select(v => v.Name));
                var confrimDialogViewModel
                    = new ConfrimDialogViewModel("存在已经添加的变量", $"变量名称:{existNames}，已经存在，是否跳过继续添加其他的变量。取消则不添加任何变量", "继续");
                var res = await _dialogService.ShowDialogAsync(confrimDialogViewModel);
                if (!res) return;
                // 从导入列表中删除已经存在的变量
                improtVariableDtos.RemoveAll(variableDto => existList.Contains(variableDto));
            }

            if (improtVariableDtos.Count != 0)
            {
                var isSuccess = await _variableAppService.BatchImportVariablesAsync(improtVariableDtos);
                if (isSuccess)
                {
                    _variableItemList.AddRange(_mapper.Map<List<VariableItemViewModel>>(improtVariableDtos));
                    NotificationHelper.ShowSuccess($"从Excel导入变量成功，共导入变量：{improtVariableDtos.Count}个");
                }
            }
            else
            {
                NotificationHelper.ShowSuccess($"列表中没有要添加的变量了。 ");
            }
        }
        catch (Exception e)
        {
            NotificationHelper.ShowError($"从TIA导入变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 从OPC UA服务器导入变量数据。
    /// 此命令通常绑定到UI中的“从OPC UA导入”按钮。
    /// </summary>
    [RelayCommand]
    private async Task ImportFromOpcUaServer()
    {
        // ContentDialog processingDialog = null; // 用于显示处理中的对话框
        // try
        // {
        //     // 检查OPC UA Endpoint URL是否已设置
        //     string opcUaEndpointUrl = VariableTable?.Device?.OpcUaEndpointUrl;
        //     if (string.IsNullOrEmpty(opcUaEndpointUrl))
        //     {
        //         NotificationHelper.ShowError("OPC UA Endpoint URL 未设置。请在设备详情中配置。");
        //         return;
        //     }
        //
        //     // 显示OPC UA导入对话框，让用户选择要导入的变量
        //     var importedVariables = await _dialogService.ShowOpcUaImportDialog(opcUaEndpointUrl);
        //     if (importedVariables == null || !importedVariables.Any())
        //     {
        //         return; // 用户取消或没有选择任何变量
        //     }
        //
        //     // 显示处理中的对话框
        //     processingDialog = _dialogService.ShowProcessingDialog("正在处理...", "正在导入OPC UA变量,请稍等片刻....");
        //
        //     // 在进行重复检查之前，先刷新 Variables 集合，确保其包含所有最新数据
        //     await RefreshDataView();
        //
        //     List<Variable> newVariables = new List<Variable>();
        //     List<string> importedVariableNames = new List<string>();
        //     List<string> existingVariableNames = new List<string>();
        //
        //     foreach (var variableData in importedVariables)
        //     {
        //         // 判断是否存在重复变量，仅在当前 VariableTable 的 Variables 中查找
        //         bool isDuplicate = Variables.Any(existingVar =>
        //                                                  (existingVar.Name == variableData.Name) ||
        //                                                  (!string.IsNullOrEmpty(variableData.NodeId) &&
        //                                                   existingVar.NodeId == variableData.NodeId) ||
        //                                                  (!string.IsNullOrEmpty(variableData.OpcUaNodeId) &&
        //                                                   existingVar.OpcUaNodeId == variableData.OpcUaNodeId)
        //         );
        //
        //         if (isDuplicate)
        //         {
        //             existingVariableNames.Add(variableData.Name);
        //         }
        //         else
        //         {
        //             variableData.CreateTime = DateTime.Now;
        //             variableData.VariableTableId = VariableTable.Id;
        //             variableData.ProtocolType = ProtocolType.OpcUA; // 确保协议类型正确
        //             variableData.IsModified = false;
        //             newVariables.Add(variableData);
        //             importedVariableNames.Add(variableData.Name);
        //         }
        //     }
        //
        //     if (newVariables.Any())
        //     {
        //         // 批量插入新变量数据到数据库
        //         var resVarDataCount = await _varDataRepository.AddAsync(newVariables);
        //         NlogHelper.Info($"成功导入OPC UA变量：{resVarDataCount}个。");
        //     }
        //
        //     // 再次刷新 Variables 集合，以反映新添加的数据
        //     await RefreshDataView();
        //
        //     processingDialog?.Hide(); // 隐藏处理中的对话框
        //
        //     // 显示导入结果对话框
        //     await _dialogService.ShowImportResultDialog(importedVariableNames, existingVariableNames);
        // }
        // catch (Exception e)
        // {
        //     // 捕获并显示错误通知
        //     NotificationHelper.ShowError($"从OPC UA服务器导入变量的过程中发生了不可预期的错误：{e.Message}", e);
        // }
        // finally
        // {
        //     processingDialog?.Hide(); // 确保在任何情况下都隐藏对话框
        // }
    }

    /// <summary>
    /// 刷新数据列表，高效地同步UI显示数据与数据库最新数据。
    /// </summary>
    private async Task RefreshDataView()
    {
        // // 从数据库加载最新的变量数据
        // var latestVariables = await _varDataRepository.GetByVariableTableIdAsync(VariableTable.Id);
        //
        // // 将最新数据转换为字典，以便快速查找
        // var latestVariablesDict = latestVariables.ToDictionary(v => v.Id);
        //
        // // 用于存储需要从 Variables 中移除的项
        // var itemsToRemove = new List<Variable>();
        //
        // // 遍历当前 Variables 集合，处理删除和更新
        // for (int i = Variables.Count - 1; i >= 0; i--)
        // {
        //     var currentVariable = Variables[i];
        //     if (latestVariablesDict.TryGetValue(currentVariable.Id, out var newVariable))
        //     {
        //         // 如果存在于最新数据中，检查是否需要更新
        //         if (!currentVariable.Equals(newVariable))
        //         {
        //             // 使用 AutoMapper 更新现有对象的属性，保持对象引用不变
        //             _mapper.Map(newVariable, currentVariable);
        //         }
        //
        //         // 从字典中移除已处理的项，剩余的将是新增项
        //         latestVariablesDict.Remove(currentVariable.Id);
        //     }
        //     else
        //     {
        //         // 如果不存在于最新数据中，则标记为删除
        //         itemsToRemove.Add(currentVariable);
        //     }
        // }
        //
        // // 移除已标记的项
        // foreach (var item in itemsToRemove)
        // {
        //     Variables.Remove(item);
        // }
        //
        // // 添加所有剩余在 latestVariablesDict 中的项（这些是新增项）
        // foreach (var newVariable in latestVariablesDict.Values)
        // {
        //     Variables.Add(newVariable);
        // }
        //
        // // 刷新视图以应用所有更改
        // VariableView.Refresh();
    }

    //
    /// <summary>
    /// 添加新的变量数据。
    /// 此命令通常绑定到UI中的“添加”按钮。
    /// </summary>
    /// <param name="variableTable">当前操作的变量表，用于设置新变量的所属ID。</param>
    [RelayCommand]
    private async void AddVariable(VariableTable variableTable)
    {
        try
        {
            // 显示添加变量数据的对话框
            VariableDialogViewModel variableDialogViewModel=new VariableDialogViewModel("添加变量","添加变量");


            var variableItemViewModel = await _dialogService.ShowDialogAsync(variableDialogViewModel);
        
            // 如果用户取消或对话框未返回数据，则直接返回
            if (variableItemViewModel == null)
                return;
        
            // // 设置新变量的所属变量表ID
            // varData.VariableTableId = variableTable.Id;
            //
            // // --- 重复性检查逻辑开始 ---
            // bool isDuplicate = false;
            // string duplicateReason = string.Empty;
            //
            // // 检查名称是否重复
            // if (Variables.Any(v => v.Name == varData.Name))
            // {
            //     isDuplicate = true;
            //     duplicateReason = $"名称 '{varData.Name}' 已存在。";
            // }
            // else
            // {
            //     // 根据协议类型检查S7地址或NodeId是否重复
            //     if (variableTable.ProtocolType == ProtocolType.S7)
            //     {
            //         if (!string.IsNullOrEmpty(varData.S7Address) &&
            //             Variables.Any(v => v.S7Address == varData.S7Address))
            //         {
            //             isDuplicate = true;
            //             duplicateReason = $"S7地址 '{varData.S7Address}' 已存在。";
            //         }
            //     }
            //     else if (variableTable.ProtocolType == ProtocolType.OpcUA)
            //     {
            //         if (!string.IsNullOrEmpty(varData.NodeId) && Variables.Any(v => v.NodeId == varData.NodeId))
            //         {
            //             isDuplicate = true;
            //             duplicateReason = $"OPC UA NodeId '{varData.NodeId}' 已存在。";
            //         }
            //     }
            // }
            //
            // if (isDuplicate)
            // {
            //     NotificationHelper.ShowError($"添加变量失败：{duplicateReason}");
            //     return;
            // }
            // // --- 重复性检查逻辑结束 ---
            //
            // // 添加变量数据到数据库
            // var resVarData = await _varDataRepository.AddAsync(varData);
            // if (resVarData == null)
            // {
            //     NotificationHelper.ShowError($"添加变量失败了:{varData?.Name}");
            //     return;
            // }
            //
            // // 更新当前页面显示的数据：将新变量添加到集合中
            // Variables.Add(resVarData);
            //
            // // 显示成功通知
            // NotificationHelper.ShowSuccess($"添加变量成功:{varData?.Name}");
        }
        catch (Exception e)
        {
            // 捕获并显示错误通知
            NotificationHelper.ShowError($"添加变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 删除选定的变量数据。
    /// 此命令通常绑定到UI中的“删除”按钮。
    /// </summary>
    /// <param name="variablesToDelete">要删除的变量数据列表。</param>
    [RelayCommand]
    public async Task DeleteVarData(List<Variable> variablesToDelete)
    {
        // // 检查是否有变量被选中
        // if (variablesToDelete == null || !variablesToDelete.Any())
        // {
        //     NotificationHelper.ShowInfo("请选择要删除的变量");
        //     return;
        // }
        //
        // // 拼接要删除的变量名称，用于确认提示
        // var names = string.Join("、", variablesToDelete.Select(v => v.Name));
        //
        // // 显示确认删除对话框
        // var confirm = await _dialogService.ShowConfrimeDialog(
        //     "删除确认",
        //     $"确定要删除选中的 {variablesToDelete.Count} 个变量吗？\n\n{names}",
        //     "删除");
        //
        // if (!confirm)
        //     return; // 如果用户取消删除，则返回
        //
        // try
        // {
        //     // 从数据库中删除变量数据
        //     var result = await _varDataRepository.DeleteAsync(variablesToDelete);
        //     if (result > 0)
        //     {
        //         await RefreshDataView();
        //         // 显示成功通知
        //         NotificationHelper.ShowSuccess($"成功删除 {result} 个变量");
        //     }
        //     else
        //     {
        //         // 显示删除失败通知
        //         NotificationHelper.ShowError("删除变量失败");
        //     }
        // }
        // catch (Exception e)
        // {
        //     // 捕获并显示错误通知
        //     NotificationHelper.ShowError($"删除变量的过程中发生了不可预期的错误：{e.Message}", e);
        // }
    }

    /// <summary>
    /// 更改选定变量的轮询频率。
    /// 此命令通常绑定到UI中的“修改轮询频率”按钮。
    /// </summary>
    /// <param name="variablesToChange">要修改轮询频率的变量数据列表。</param>
    [RelayCommand]
    public async Task ChangePollLevel(IList<object> variablesToChange)
    {
        // var validVariables = variablesToChange?.OfType<Variable>()
        //                                       .ToList();
        //
        // // 检查是否有变量被选中
        // if (validVariables == null || !validVariables.Any())
        // {
        //     NotificationHelper.ShowInfo("请选择要修改轮询频率的变量");
        //     return;
        // }
        //
        // // 显示轮询频率选择对话框，并传入第一个变量的当前轮询频率作为默认值
        // var newPollLevelType = await _dialogService.ShowPollLevelDialog(validVariables.First()
        //                                                                     .PollLevelType);
        // if (newPollLevelType.HasValue)
        // {
        //     // 更新所有选定变量的轮询频率和修改状态
        //     foreach (var variable in validVariables)
        //     {
        //         variable.PollLevelType = newPollLevelType.Value;
        //         variable.IsModified = false; // 标记为未修改，因为已保存到数据库
        //     }
        //
        //     // 批量更新数据库中的变量数据
        //     await _varDataRepository.UpdateAsync(validVariables);
        //
        //     await RefreshDataView();
        //     // 显示成功通知
        //     NotificationHelper.ShowSuccess($"已成功更新 {validVariables.Count} 个变量的轮询频率");
        // }
    }

    /// <summary>
    /// 修改选定变量的OPC UA更新方式（轮询或订阅）。
    /// </summary>
    /// <param name="variablesToChange">要修改更新方式的变量数据列表。</param>
    [RelayCommand]
    public async Task ModifyOpcUaUpdateType(IList<object> variablesToChange)
    {
        // // 过滤出有效的VariableData对象
        // var validVariables = variablesToChange?.OfType<Variable>()
        //                                       .ToList();
        //
        // if (validVariables == null || !validVariables.Any())
        // {
        //     NotificationHelper.ShowInfo("请选择要修改更新方式的OPC UA变量");
        //     return;
        // }
        //
        //
        // // 显示更新方式选择对话框
        // var newUpdateType = await _dialogService.ShowOpcUaUpdateTypeDialog();
        // if (newUpdateType.HasValue)
        // {
        //     // 更新所有选定变量的更新方式
        //     foreach (var variable in validVariables)
        //     {
        //         variable.OpcUaUpdateType = newUpdateType.Value;
        //     }
        //
        //     // 批量更新数据库
        //     await _varDataRepository.UpdateAsync(validVariables);
        //     NotificationHelper.ShowSuccess($"已成功为 {validVariables.Count} 个变量更新OPC UA更新方式。");
        // }
    }

    /// <summary>
    /// 为选定的变量添加MQTT服务器。
    /// 此命令通常绑定到UI中的“添加MQTT服务器”按钮。
    /// </summary>
    /// <param name="variablesToAddMqtt">要添加MQTT服务器的变量数据列表。</param>
    [RelayCommand]
    public async Task AddMqttServerToVariables(IList<object> variablesToAddMqtt)
    {
        // var validVariables = variablesToAddMqtt?.OfType<Variable>()
        //                                        .ToList();
        //
        // // 检查是否有变量被选中
        // if (validVariables == null || !validVariables.Any())
        // {
        //     NotificationHelper.ShowInfo("请选择要添加MQTT服务器的变量");
        //     return;
        // }
        //
        // try
        // {
        //     // 显示MQTT服务器选择对话框，让用户选择一个MQTT服务器
        //     var selectedMqtt = await _dialogService.ShowMqttSelectionDialog();
        //     if (selectedMqtt == null)
        //     {
        //         return; // 用户取消选择
        //     }
        //
        //     // 显示批量编辑别名对话框
        //     var editedVariableMqtts = await _dialogService.ShowMqttAliasBatchEditDialog(validVariables, selectedMqtt);
        //
        //     if (editedVariableMqtts == null || !editedVariableMqtts.Any())
        //     {
        //         NotificationHelper.ShowInfo("没有变量别名被设置或已取消。");
        //         return;
        //     }
        //
        //
        //     int totalAffectedCount = 0;
        //     // 调用仓库方法来添加或更新MQTT服务器关联和别名
        //     var resCount = await _varDataRepository.AddMqttToVariablesAsync(editedVariableMqtts);
        //     totalAffectedCount += resCount;
        //     
        //
        //     //更新变量Variable的VariableMqtts列表
        //     foreach (var editedVariableMqtt in editedVariableMqtts)
        //     {
        //             // 更新内存中的 Variable 对象
        //             var originalVariable = VariableTable.Variables.FirstOrDefault(v=>v.Id==editedVariableMqtt.Variable.Id);
        //             if (originalVariable == null)
        //             {
        //                 NlogHelper.Warn($"没有在VariableTable.Variables中找到,ID:{editedVariableMqtt.Variable.Id},Name:{editedVariableMqtt.Variable.Name}的对象");
        //                 continue;
        //             }
        //             
        //             
        //             if (originalVariable.VariableMqtts == null)
        //             {
        //                 originalVariable.VariableMqtts = new List<VariableMqtt>();
        //             }
        //
        //             // 检查是否已存在该变量与该MQTT服务器的关联
        //             var existingVariableMqtt
        //                 = originalVariable.VariableMqtts.FirstOrDefault(vm => vm.MqttId ==
        //                                                                       editedVariableMqtt.Mqtt.Id);
        //
        //             if (existingVariableMqtt == null)
        //             {
        //                 // 如果不存在，则添加新的关联
        //                 var variableMqtt = new VariableMqtt(originalVariable,editedVariableMqtt.Mqtt)
        //                                    {
        //                                        VariableId = originalVariable.Id,
        //                                        MqttId = editedVariableMqtt.Mqtt.Id,
        //                                        MqttAlias = editedVariableMqtt.MqttAlias,
        //                                        Mqtt = editedVariableMqtt.Mqtt // 关联Mqtt对象，方便UI显示
        //                                    };
        //                 originalVariable.VariableMqtts.Add(variableMqtt);
        //                 //更新MQTT服务器对应的的VariableMqtts列表
        //                 selectedMqtt.VariableMqtts.Add(variableMqtt);
        //             }
        //             else
        //             {
        //                 // 如果存在，则更新别名
        //                 existingVariableMqtt.MqttAlias = editedVariableMqtt.MqttAlias;
        //             }
        //     }
        //
        //
        //     if (totalAffectedCount > 0)
        //     {
        //         // 刷新界面以反映更改
        //         await RefreshDataView();
        //         NotificationHelper.ShowSuccess($"已成功为 {totalAffectedCount} 个变量添加/更新MQTT服务器: {selectedMqtt.Name} 的别名。");
        //     }
        //     else
        //     {
        //         NotificationHelper.ShowInfo($"没有新的变量关联或别名更新到MQTT服务器: {selectedMqtt.Name}。");
        //     }
        // }
        // catch (Exception ex)
        // {
        //     // 捕获并显示错误通知
        //     NotificationHelper.ShowError($"添加MQTT服务器失败: {ex.Message}", ex);
        // }
    }

    /// <summary>
    /// 修改选定变量的启用状态。
    /// </summary>
    /// <param name="variablesToChange">要修改启用状态的变量数据列表。</param>
    [RelayCommand]
    public async Task ModifyIsActive(IList<object> variablesToChange)
    {
        // var validVariables = variablesToChange?.OfType<Variable>()
        //                                       .ToList();
        //
        // if (validVariables == null || !validVariables.Any())
        // {
        //     NotificationHelper.ShowInfo("请选择要修改启用状态的变量");
        //     return;
        // }
        //
        // // 假设所有选中的变量都应该被设置为相同的状态，取第一个变量的当前状态的反值
        // var currentIsActive = validVariables.First()
        //                                     .IsActive;
        // var newIsActive = !currentIsActive;
        //
        // var confirm = await _dialogService.ShowIsActiveDialog(newIsActive);
        //
        // if (confirm.HasValue && confirm.Value == newIsActive)
        // {
        //     foreach (var variable in validVariables)
        //     {
        //         variable.IsActive = newIsActive;
        //     }
        //
        //     await _varDataRepository.UpdateAsync(validVariables);
        //
        //     // 更新界面
        //     await RefreshDataView();
        //
        //
        //     NotificationHelper.ShowSuccess($"已成功将 {validVariables.Count} 个变量的启用状态修改为 {newIsActive}");
        // }
        // else
        // {
        //     NotificationHelper.ShowInfo("操作已取消或状态未改变。");
        // }
    }

    /// <summary>
    /// 当变量表的启用/禁用状态改变时调用。
    /// 更新数据库中变量表的激活状态，并显示相应的通知。
    /// </summary>
    /// <param name="active">变量表的新激活状态（true为启用，false为禁用）。</param>
    public async Task OnIsActiveChanged(bool active)
    {
        // // 更新数据库中变量表的激活状态
        // var res = await _varTableRepository.UpdateAsync(VariableTable);
        // if (res > 0)
        // {
        //     // 根据激活状态显示成功通知
        //     var statusMessage = active ? "已启用" : "已停用";
        //     NotificationHelper.ShowSuccess($"变量表：{VariableTable.Name},{statusMessage}");
        // }
        // else
        // {
        //     // 显示失败通知
        //     NotificationHelper.ShowError($"变量表：{VariableTable.Name},状态修改失败，状态：{active}");
        //     // _logger.LogInformation($"变量表：{VariableTable.Name},状态修改失败，状态：{active}"); // 可以选择记录日志
        // }
    }

    public async Task OnNavigatedToAsync(MenuItemViewModel menu)
    {
        var varTable = _dataServices.VariableTables.FirstOrDefault(v => v.Id == menu.TargetId);

        if (varTable != null)
        {
            CurrentVariableTable = varTable;
            // 根据变量表的协议类型设置对应的布尔属性

            if (CurrentVariableTable.Protocol == ProtocolType.S7)
            {
                IsS7ProtocolSelected = true;
            }
            else if (CurrentVariableTable.Protocol == ProtocolType.OpcUa)
            {
                IsOpcUaProtocolSelected = true;
            }

            OnLoaded();
        }
    }
}