using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using AutoMapper;
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
using SqlSugar;

namespace PMSWPF.ViewModels;

/// <summary>
/// VariableTableViewModel 是用于管理和显示变量表数据的视图模型。
/// 它与 VariableTableView 视图进行数据绑定，并处理用户交互逻辑。
///
/// 调用逻辑概述：
/// 1.  **实例化**: 当导航到 VariableTableView 时，通常会通过依赖注入框架（如 CommunityToolkit.Mvvm 的服务定位器或自定义工厂）实例化 VariableTableViewModel。
///     构造函数 <see cref="VariableTableViewModel(IDialogService)"/> 负责初始化必要的服务和数据仓库。
/// 2.  **数据加载**: 
///     -   当视图加载完成时，框架会自动调用 <see cref="OnLoaded"/> 方法。
///     -   此方法会根据传入的 <see cref="VariableTable"/> 对象初始化 <see cref="DataVariables"/> 集合，并设置协议类型相关的布尔属性。
///     -   它还会创建 <see cref="_originalDataVariables"/> 的深拷贝，用于在用户取消保存时还原数据。
/// 3.  **数据绑定与显示**: 
///     -   <see cref="VariableTable"/> 属性绑定到视图中显示的当前变量表信息。
///     -   <see cref="DataVariables"/> 属性（ObservableCollection）绑定到视图中的数据网格或列表，用于显示变量数据。
///     -   <see cref="VariableDataView"/> 是一个 ICollectionView，用于支持数据过滤（通过 <see cref="FilterVariables"/> 方法）和排序。
///     -   <see cref="SearchText"/> 属性绑定到搜索框，当其值改变时，会自动触发 <see cref="OnSearchTextChanged(string)"/> 方法刷新视图。
/// 4.  **用户交互与命令**: 
///     -   视图中的按钮和其他交互元素通过 `RelayCommand` 绑定到 ViewModel 中的命令方法。
///     -   例如，"保存"按钮可能绑定到 <see cref="SaveModifiedVarDataCommand"/>，"编辑"按钮绑定到 <see cref="EditVarDataCommand"/> 等。
///     -   这些命令方法负责执行业务逻辑，如更新数据库、显示对话框、导入数据等。
/// 5.  **对话框交互**: 
///     -   ViewModel 通过注入的 <see cref="IDialogService"/> 接口与各种对话框进行交互，例如确认对话框、编辑对话框、导入对话框等。
///     -   对话框的显示和结果处理都在 ViewModel 中完成。
/// 6.  **数据保存与退出**: 
///     -   当用户尝试离开当前视图时，框架会调用 <see cref="OnExitAsync"/> 方法。
///     -   此方法会检查是否有未保存的修改，并提示用户是否保存或放弃更改。
///     -   <see cref="SaveModifiedVarDataCommand"/> 用于显式保存修改。
/// 7.  **通知**: 
///     -   通过 <see cref="NotificationHelper"/> 显示成功、错误或信息提示给用户。
/// 8.  **协议类型切换**: 
///     -   <see cref="IsS7ProtocolSelected"/> 和 <see cref="IsOpcUaProtocolSelected"/> 属性用于控制视图中与协议相关的UI元素的可见性或状态。
/// </summary>
partial class VariableTableViewModel : ViewModelBase
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 对话服务接口，用于显示各种对话框（如确认、编辑、导入等）。
    /// </summary>
    private readonly IDialogService _dialogService;

    /// <summary>
    /// 当前正在操作的变量表实体。
    /// 通过 ObservableProperty 自动生成 VariableTable 属性和 OnVariableTableChanged 方法。
    /// </summary>
    [ObservableProperty]
    private VariableTable variableTable;

    /// <summary>
    /// 存储当前变量表中的所有变量数据的集合。
    /// 通过 ObservableProperty 自动生成 DataVariables 属性和 OnDataVariablesChanged 方法。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<VariableData> _dataVariables;

    /// <summary>
    /// 当前选中的变量数据。
    /// 通过 ObservableProperty 自动生成 SelectedVariableData 属性和 OnSelectedVariableDataChanged 方法。
    /// </summary>
    [ObservableProperty]
    private VariableData _selectedVariableData;

    /// <summary>
    /// 用于过滤变量数据的搜索文本。
    /// 通过 ObservableProperty 自动生成 SearchText 属性和 OnSearchTextChanged 方法。
    /// </summary>
    [ObservableProperty]
    private string _searchText;

    /// <summary>
    /// 用于在UI中显示和过滤变量数据的视图集合。
    /// </summary>
    [ObservableProperty]
    private ICollectionView variableDataView;

    /// <summary>
    /// 指示视图是否已完成首次加载。
    /// 用于防止某些UI控件（如ToggleSwitch）在初始化时触发不必要的事件。
    /// </summary>
    public bool IsLoadCompletion { get; set; } = false;

    /// <summary>
    /// 变量表数据仓库，用于与变量表相关的数据库操作。
    /// </summary>
    private readonly VarTableRepository _varTableRepository;

    /// <summary>
    /// 变量数据仓库，用于与变量数据相关的数据库操作。
    /// </summary>
    private readonly VarDataRepository _varDataRepository;

    /// <summary>
    /// 原始变量数据的深拷贝备份，用于在用户取消保存时还原数据。
    /// </summary>
    private ObservableCollection<VariableData>? _originalDataVariables;

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

    public VariableTableViewModel(IMapper mapper, IDialogService dialogService, VarTableRepository varTableRepository,
                                  VarDataRepository varDataRepository, DataServices dataServices)
    {
        _mapper = mapper;
        _dialogService = dialogService;
        _dataServices = dataServices;
        IsLoadCompletion = false; // 初始设置为 false，表示未完成加载
        _varTableRepository = varTableRepository;
        _varDataRepository = varDataRepository;
        _dataVariables = new ObservableCollection<VariableData>(); // 初始化集合
        VariableDataView = CollectionViewSource.GetDefaultView(DataVariables); // 获取集合视图
        VariableDataView.Filter = FilterVariables; // 设置过滤方法
    }

    /// <summary>
    /// 用于过滤 <see cref="VariableDataView"/> 中的变量数据。
    /// 根据 <see cref="SearchText"/> 属性的值进行模糊匹配。
    /// </summary>
    /// <param name="item">要过滤的集合中的单个项。</param>
    /// <returns>如果项匹配搜索条件则为 true，否则为 false。</returns>
    private bool FilterVariables(object item)
    {
        // 如果搜索文本为空或空白，则显示所有项
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        // 尝试将项转换为 VariableData 类型
        if (item is VariableData variable)
        {
            var searchTextLower = SearchText.ToLower();
            // 检查变量的名称、描述、NodeId、S7地址、数据值或显示值是否包含搜索文本
            return variable.Name?.ToLower()
                           .Contains(searchTextLower) == true ||
                   variable.Description?.ToLower()
                           .Contains(searchTextLower) == true ||
                   variable.NodeId?.ToLower()
                           .Contains(searchTextLower) == true ||
                   variable.S7Address?.ToLower()
                           .Contains(searchTextLower) == true ||
                   variable.DataValue?.ToLower()
                           .Contains(searchTextLower) == true ||
                   variable.DisplayValue?.ToLower()
                           .Contains(searchTextLower) == true;
        }

        return false;
    }

    /// <summary>
    /// 当 <see cref="SearchText"/> 属性的值发生改变时自动调用。
    /// 刷新 <see cref="VariableDataView"/> 以应用新的过滤条件。
    /// </summary>
    /// <param name="value">新的搜索文本值。</param>
    partial void OnSearchTextChanged(string value)
    {
        VariableDataView?.Refresh();
    }

    /// <summary>
    /// 当视图加载完成时调用。
    /// 初始化协议选择状态，加载变量数据，并创建原始数据的深拷贝备份。
    /// </summary>
    public override void OnLoaded()
    {
        // 根据变量表的协议类型设置对应的布尔属性
        IsS7ProtocolSelected = VariableTable.ProtocolType == ProtocolType.S7;
        IsOpcUaProtocolSelected = VariableTable.ProtocolType == ProtocolType.OpcUA;

        // 如果变量表包含数据变量，则进行初始化
        if (VariableTable.DataVariables != null)
        {
            // // 将变量表中的数据变量复制到可观察集合中
            DataVariables.Clear(); // 清空现有集合
            foreach (var item in VariableTable.DataVariables)
            {
                DataVariables.Add(item); // 添加新项
            }


            VariableDataView.Refresh(); // 刷新视图以应用过滤和排序

            // 创建原始数据的深拷贝备份，用于在取消保存时还原
            var serialized = JsonConvert.SerializeObject(DataVariables);
            _originalDataVariables = JsonConvert.DeserializeObject<ObservableCollection<VariableData>>(serialized);

            // 在数据加载完成后，将所有变量的 IsModified 状态重置为 false
            foreach (var variableData in DataVariables)
            {
                variableData.IsModified = false;
            }
        }

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
        var modifiedDatas = DataVariables.Where(d => d.IsModified == true)
                                         .ToList();
        // 如果没有修改，则直接允许退出
        if (modifiedDatas.Count == 0)
            return true;

        // 提示用户有未保存的数据，询问是否离开
        var isExit = await _dialogService.ShowConfrimeDialog(
            "数据未保存", $"你有{modifiedDatas.Count}个修改的变量没有保存，离开后这些数据就可能丢失了确认要离开吗？", "离开");

        // 如果用户选择不离开（即不保存），则还原数据
        if (!isExit)
        {
            // 遍历所有已修改的数据，从原始备份中还原
            foreach (var modifiedData in modifiedDatas)
            {
                var oldData = _originalDataVariables.First(od => od.Id == modifiedData.Id);
                // 将原始数据复制回当前数据
                _mapper.Map(oldData, modifiedData);
                modifiedData.IsModified = false; // 重置修改状态
            }

            return false; // 不允许退出
        }

        return true; // 允许退出
    }

    /// <summary>
    /// 保存所有已修改的变量数据到数据库。
    /// 此命令通常绑定到UI中的“保存”按钮。
    /// </summary>
    [RelayCommand]
    private async void SaveModifiedVarData()
    {
        // 查找所有已标记为修改的变量数据
        var modifiedDatas = DataVariables.Where(d => d.IsModified == true)
                                         .ToList();
        // 更新数据库中的这些数据
        await _varDataRepository.UpdateAsync(modifiedDatas);

        // 还原所有已保存数据的修改状态
        foreach (var modifiedData in modifiedDatas)
        {
            modifiedData.IsModified = false;
        }

        // 显示成功通知
        NotificationHelper.ShowSuccess($"修改的{modifiedDatas.Count}变量保存成功.");
    }

    /// <summary>
    /// 编辑选定的变量数据。
    /// 此命令通常绑定到UI中的“编辑”按钮或双击事件。
    /// </summary>
    /// <param name="variableTable">当前操作的变量表，用于更新其内部的变量数据。</param>
    [RelayCommand]
    private async void EditVarData(VariableTable variableTable)
    {
        try
        {
            // 显示编辑变量数据的对话框，并传入当前选中的变量数据
            var varData = await _dialogService.ShowEditVarDataDialog(SelectedVariableData);

            // 如果用户取消或对话框未返回数据，则直接返回
            if (varData == null)
                return;

            // 设置变量数据的所属变量表ID
            varData.VariableTableId = variableTable.Id;

            // 更新数据库中的变量数据
            await _varDataRepository.UpdateAsync(varData);

            // 更新当前页面显示的数据：找到原数据在集合中的索引并替换
            var index = variableTable.DataVariables.IndexOf(SelectedVariableData);
            if (index >= 0 && index < variableTable.DataVariables.Count)
                variableTable.DataVariables[index] = varData; // 替换为编辑后的数据

            // 显示成功通知
            NotificationHelper.ShowSuccess($"编辑变量成功:{varData?.Name}");
        }
        catch (Exception e)
        {
            // 捕获并显示错误通知
            NotificationHelper.ShowError($"编辑变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 从TIA Portal导出的变量表Excel文件中导入变量数据。
    /// 此命令通常绑定到UI中的“从TIA导入”按钮。
    /// </summary>
    [RelayCommand]
    private async void ImprotFromTiaVarTable()
    {
        ContentDialog processingDialog = null; // 用于显示处理中的对话框
        try
        {
            // 让用户选择要导入的Excel文件路径
            var filePath = await _dialogService.ShowImportExcelDialog();
            if (string.IsNullOrEmpty(filePath))
                return; // 如果用户取消选择，则返回

            // 读取Excel文件并将其内容转换为 VariableData 列表
            var importVarDataList = ExcelHelper.ImprotFromTiaVariableTable(filePath);
            if (importVarDataList.Count == 0)
                return; // 如果没有读取到数据，则返回

            // 显示处理中的对话框
            processingDialog = _dialogService.ShowProcessingDialog("正在处理...", "正在导入变量,请稍等片刻....");

            List<VariableData> newVariables = new List<VariableData>();
            List<string> importedVariableNames = new List<string>();
            List<string> existingVariableNames = new List<string>();

            foreach (var variableData in importVarDataList)
            {
                // 判断是否存在重复变量
                // 判断是否存在重复变量，仅在当前 VariableTable 的 DataVariables 中查找
                bool isDuplicate = DataVariables.Any(existingVar =>
                                                         (existingVar.Name == variableData.Name) ||
                                                         (!string.IsNullOrEmpty(variableData.NodeId) &&
                                                          existingVar.NodeId == variableData.NodeId) ||
                                                         (!string.IsNullOrEmpty(variableData.S7Address) &&
                                                          existingVar.S7Address == variableData.S7Address)
                );

                if (isDuplicate)
                {
                    existingVariableNames.Add(variableData.Name);
                }
                else
                {
                    variableData.CreateTime = DateTime.Now;
                    variableData.IsActive = true;
                    variableData.VariableTableId = VariableTable.Id;
                    newVariables.Add(variableData);
                    importedVariableNames.Add(variableData.Name);
                }
            }

            if (newVariables.Any())
            {
                // 批量插入新变量数据到数据库
                var resVarDataCount = await _varDataRepository.AddAsync(newVariables);
                NlogHelper.Info($"成功导入变量：{resVarDataCount}个。");
            }

            // 更新界面显示的数据：重新从数据库加载所有变量数据
            await RefreshDataView();

            processingDialog?.Hide(); // 隐藏处理中的对话框

            // 显示导入结果对话框
            await _dialogService.ShowImportResultDialog(importedVariableNames, existingVariableNames);
        }
        catch (Exception e)
        {
            // 捕获并显示错误通知
            NotificationHelper.ShowError($"从TIA导入变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
        finally
        {
            processingDialog?.Hide(); // 确保在任何情况下都隐藏对话框
        }
    }

    /// <summary>
    /// 从OPC UA服务器导入变量数据。
    /// 此命令通常绑定到UI中的“从OPC UA导入”按钮。
    /// </summary>
    [RelayCommand]
    private async Task ImportFromOpcUaServer()
    {
        ContentDialog processingDialog = null; // 用于显示处理中的对话框
        try
        {
            // 检查OPC UA Endpoint URL是否已设置
            string opcUaEndpointUrl = VariableTable?.Device?.OpcUaEndpointUrl;
            if (string.IsNullOrEmpty(opcUaEndpointUrl))
            {
                NotificationHelper.ShowError("OPC UA Endpoint URL 未设置。请在设备详情中配置。");
                return;
            }

            // 显示OPC UA导入对话框，让用户选择要导入的变量
            var importedVariables = await _dialogService.ShowOpcUaImportDialog(opcUaEndpointUrl);
            if (importedVariables == null || !importedVariables.Any())
            {
                return; // 用户取消或没有选择任何变量
            }

            // 显示处理中的对话框
            processingDialog = _dialogService.ShowProcessingDialog("正在处理...", "正在导入OPC UA变量,请稍等片刻....");

            // 在进行重复检查之前，先刷新 DataVariables 集合，确保其包含所有最新数据
            await RefreshDataView();

            List<VariableData> newVariables = new List<VariableData>();
            List<string> importedVariableNames = new List<string>();
            List<string> existingVariableNames = new List<string>();

            foreach (var variableData in importedVariables)
            {
                // 判断是否存在重复变量，仅在当前 VariableTable 的 DataVariables 中查找
                bool isDuplicate = DataVariables.Any(existingVar =>
                                                         (existingVar.Name == variableData.Name) ||
                                                         (!string.IsNullOrEmpty(variableData.NodeId) &&
                                                          existingVar.NodeId == variableData.NodeId) ||
                                                         (!string.IsNullOrEmpty(variableData.OpcUaNodeId) &&
                                                          existingVar.OpcUaNodeId == variableData.OpcUaNodeId)
                );

                if (isDuplicate)
                {
                    existingVariableNames.Add(variableData.Name);
                }
                else
                {
                    variableData.CreateTime = DateTime.Now;
                    variableData.VariableTableId = VariableTable.Id;
                    variableData.ProtocolType = ProtocolType.OpcUA; // 确保协议类型正确
                    variableData.IsModified = false;
                    newVariables.Add(variableData);
                    importedVariableNames.Add(variableData.Name);
                }
            }

            if (newVariables.Any())
            {
                // 批量插入新变量数据到数据库
                var resVarDataCount = await _varDataRepository.AddAsync(newVariables);
                NlogHelper.Info($"成功导入OPC UA变量：{resVarDataCount}个。");
            }

            // 再次刷新 DataVariables 集合，以反映新添加的数据
            await RefreshDataView();

            processingDialog?.Hide(); // 隐藏处理中的对话框

            // 显示导入结果对话框
            await _dialogService.ShowImportResultDialog(importedVariableNames, existingVariableNames);
        }
        catch (Exception e)
        {
            // 捕获并显示错误通知
            NotificationHelper.ShowError($"从OPC UA服务器导入变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
        finally
        {
            processingDialog?.Hide(); // 确保在任何情况下都隐藏对话框
        }
    }

    /// <summary>
    /// 刷新数据列表，高效地同步UI显示数据与数据库最新数据。
    /// </summary>
    private async Task RefreshDataView()
    {
        // 从数据库加载最新的变量数据
        var latestVariables = await _varDataRepository.GetByVariableTableIdAsync(VariableTable.Id);

        // 将最新数据转换为字典，以便快速查找
        var latestVariablesDict = latestVariables.ToDictionary(v => v.Id);

        // 用于存储需要从 DataVariables 中移除的项
        var itemsToRemove = new List<VariableData>();

        // 遍历当前 DataVariables 集合，处理删除和更新
        for (int i = DataVariables.Count - 1; i >= 0; i--)
        {
            var currentVariable = DataVariables[i];
            if (latestVariablesDict.TryGetValue(currentVariable.Id, out var newVariable))
            {
                // 如果存在于最新数据中，检查是否需要更新
                if (!currentVariable.Equals(newVariable))
                {
                    // 使用 AutoMapper 更新现有对象的属性，保持对象引用不变
                    _mapper.Map(newVariable, currentVariable);
                }

                // 从字典中移除已处理的项，剩余的将是新增项
                latestVariablesDict.Remove(currentVariable.Id);
            }
            else
            {
                // 如果不存在于最新数据中，则标记为删除
                itemsToRemove.Add(currentVariable);
            }
        }

        // 移除已标记的项
        foreach (var item in itemsToRemove)
        {
            DataVariables.Remove(item);
        }

        // 添加所有剩余在 latestVariablesDict 中的项（这些是新增项）
        foreach (var newVariable in latestVariablesDict.Values)
        {
            DataVariables.Add(newVariable);
        }

        // 刷新视图以应用所有更改
        VariableDataView.Refresh();
    }

    //
    /// <summary>
    /// 添加新的变量数据。
    /// 此命令通常绑定到UI中的“添加”按钮。
    /// </summary>
    /// <param name="variableTable">当前操作的变量表，用于设置新变量的所属ID。</param>
    [RelayCommand]
    private async void AddVarData(VariableTable variableTable)
    {
        try
        {
            // 显示添加变量数据的对话框
            var varData = await _dialogService.ShowAddVarDataDialog();

            // 如果用户取消或对话框未返回数据，则直接返回
            if (varData == null)
                return;

            // 设置新变量的所属变量表ID
            varData.VariableTableId = variableTable.Id;

            // --- 重复性检查逻辑开始 ---
            bool isDuplicate = false;
            string duplicateReason = string.Empty;

            // 检查名称是否重复
            if (DataVariables.Any(v => v.Name == varData.Name))
            {
                isDuplicate = true;
                duplicateReason = $"名称 '{varData.Name}' 已存在。";
            }
            else
            {
                // 根据协议类型检查S7地址或NodeId是否重复
                if (variableTable.ProtocolType == ProtocolType.S7)
                {
                    if (!string.IsNullOrEmpty(varData.S7Address) &&
                        DataVariables.Any(v => v.S7Address == varData.S7Address))
                    {
                        isDuplicate = true;
                        duplicateReason = $"S7地址 '{varData.S7Address}' 已存在。";
                    }
                }
                else if (variableTable.ProtocolType == ProtocolType.OpcUA)
                {
                    if (!string.IsNullOrEmpty(varData.NodeId) && DataVariables.Any(v => v.NodeId == varData.NodeId))
                    {
                        isDuplicate = true;
                        duplicateReason = $"OPC UA NodeId '{varData.NodeId}' 已存在。";
                    }
                }
            }

            if (isDuplicate)
            {
                NotificationHelper.ShowError($"添加变量失败：{duplicateReason}");
                return;
            }
            // --- 重复性检查逻辑结束 ---

            // 添加变量数据到数据库
            var resVarData = await _varDataRepository.AddAsync(varData);
            if (resVarData == null)
            {
                NotificationHelper.ShowError($"添加变量失败了:{varData?.Name}");
                return;
            }

            // 更新当前页面显示的数据：将新变量添加到集合中
            DataVariables.Add(resVarData);

            // 显示成功通知
            NotificationHelper.ShowSuccess($"添加变量成功:{varData?.Name}");
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
    public async Task DeleteVarData(List<VariableData> variablesToDelete)
    {
        // 检查是否有变量被选中
        if (variablesToDelete == null || !variablesToDelete.Any())
        {
            NotificationHelper.ShowInfo("请选择要删除的变量");
            return;
        }

        // 拼接要删除的变量名称，用于确认提示
        var names = string.Join("、", variablesToDelete.Select(v => v.Name));

        // 显示确认删除对话框
        var confirm = await _dialogService.ShowConfrimeDialog(
            "删除确认",
            $"确定要删除选中的 {variablesToDelete.Count} 个变量吗？\n\n{names}",
            "删除");

        if (!confirm)
            return; // 如果用户取消删除，则返回

        try
        {
            // 从数据库中删除变量数据
            var result = await _varDataRepository.DeleteAsync(variablesToDelete);
            if (result > 0)
            {
                await RefreshDataView();
                // 显示成功通知
                NotificationHelper.ShowSuccess($"成功删除 {result} 个变量");
            }
            else
            {
                // 显示删除失败通知
                NotificationHelper.ShowError("删除变量失败");
            }
        }
        catch (Exception e)
        {
            // 捕获并显示错误通知
            NotificationHelper.ShowError($"删除变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 更改选定变量的轮询频率。
    /// 此命令通常绑定到UI中的“修改轮询频率”按钮。
    /// </summary>
    /// <param name="variablesToChange">要修改轮询频率的变量数据列表。</param>
    [RelayCommand]
    public async Task ChangePollLevel(IList<object> variablesToChange)
    {
        var validVariables = variablesToChange?.OfType<VariableData>()
                                              .ToList();

        // 检查是否有变量被选中
        if (validVariables == null || !validVariables.Any())
        {
            NotificationHelper.ShowInfo("请选择要修改轮询频率的变量");
            return;
        }

        // 显示轮询频率选择对话框，并传入第一个变量的当前轮询频率作为默认值
        var newPollLevelType = await _dialogService.ShowPollLevelDialog(validVariables.First()
                                                                            .PollLevelType);
        if (newPollLevelType.HasValue)
        {
            // 更新所有选定变量的轮询频率和修改状态
            foreach (var variable in validVariables)
            {
                variable.PollLevelType = newPollLevelType.Value;
                variable.IsModified = false; // 标记为未修改，因为已保存到数据库
            }

            // 批量更新数据库中的变量数据
            await _varDataRepository.UpdateAsync(validVariables);
            // 显示成功通知
            NotificationHelper.ShowSuccess($"已成功更新 {validVariables.Count} 个变量的轮询频率");
        }
    }

    /// <summary>
    /// 修改选定变量的OPC UA更新方式（轮询或订阅）。
    /// </summary>
    /// <param name="variablesToChange">要修改更新方式的变量数据列表。</param>
    [RelayCommand]
    public async Task ModifyOpcUaUpdateType(IList<object> variablesToChange)
    {
        // 过滤出有效的VariableData对象
        var validVariables = variablesToChange?.OfType<VariableData>()
                                              .ToList();

        if (validVariables == null || !validVariables.Any())
        {
            NotificationHelper.ShowInfo("请选择要修改更新方式的OPC UA变量");
            return;
        }


        // 显示更新方式选择对话框
        var newUpdateType = await _dialogService.ShowOpcUaUpdateTypeDialog();
        if (newUpdateType.HasValue)
        {
            // 更新所有选定变量的更新方式
            foreach (var variable in validVariables)
            {
                variable.OpcUaUpdateType = newUpdateType.Value;
            }

            // 批量更新数据库
            await _varDataRepository.UpdateAsync(validVariables);
            NotificationHelper.ShowSuccess($"已成功为 {validVariables.Count} 个变量更新OPC UA更新方式。");
        }
    }

    /// <summary>
    /// 为选定的变量添加MQTT服务器。
    /// 此命令通常绑定到UI中的“添加MQTT服务器”按钮。
    /// </summary>
    /// <param name="variablesToAddMqtt">要添加MQTT服务器的变量数据列表。</param>
    [RelayCommand]
    public async Task AddMqttServerToVariables(IList<object> variablesToAddMqtt)
    {
        var validVariables = variablesToAddMqtt?.OfType<VariableData>()
                                               .ToList();

        // 检查是否有变量被选中
        if (validVariables == null || !validVariables.Any())
        {
            NotificationHelper.ShowInfo("请选择要添加MQTT服务器的变量");
            return;
        }

        try
        {
            // 显示MQTT服务器选择对话框，让用户选择一个MQTT服务器
            var selectedMqtt = await _dialogService.ShowMqttSelectionDialog();
            if (selectedMqtt == null)
            {
                return; // 用户取消选择
            }

            // 显示批量编辑别名对话框
            var editedVariableMqtts = await _dialogService.ShowMqttAliasBatchEditDialog(validVariables, selectedMqtt);

            if (editedVariableMqtts == null || !editedVariableMqtts.Any())
            {
                NotificationHelper.ShowInfo("没有变量别名被设置或已取消。");
                return;
            }


            int totalAffectedCount = 0;
            // 调用仓库方法来添加或更新MQTT服务器关联和别名
            var affectedCount = await _varDataRepository.AddMqttToVariablesAsync(editedVariableMqtts);
            totalAffectedCount += affectedCount;
            if (affectedCount == 0)
            {
                NotificationHelper.ShowInfo("没有任何要添加或者更新的MQTT服务器.");
                return;
            }

            foreach (var editedVariableMqtt in editedVariableMqtts)
            {
                
                    // 更新内存中的 VariableData 对象
                    var originalVariable = editedVariableMqtt.VariableData;
                    if (originalVariable.VariableMqtts == null)
                    {
                        originalVariable.VariableMqtts = new List<VariableMqtt>();
                    }

                    // 检查是否已存在该变量与该MQTT服务器的关联
                    var existingVariableMqtt
                        = originalVariable.VariableMqtts.FirstOrDefault(vm => vm.MqttId ==
                                                                              editedVariableMqtt.Mqtt.Id);

                    if (existingVariableMqtt == null)
                    {
                        // 如果不存在，则添加新的关联
                        originalVariable.VariableMqtts.Add(new VariableMqtt
                                                           {
                                                               VariableDataId = originalVariable.Id,
                                                               MqttId = editedVariableMqtt.Mqtt.Id,
                                                               MqttAlias = editedVariableMqtt.MqttAlias,
                                                               Mqtt = editedVariableMqtt.Mqtt // 关联Mqtt对象，方便UI显示
                                                           });
                    }
                    else
                    {
                        // 如果存在，则更新别名
                        existingVariableMqtt.MqttAlias = editedVariableMqtt.MqttAlias;
                    }
            }


            if (totalAffectedCount > 0)
            {
                // 刷新界面以反映更改
                await RefreshDataView();
                NotificationHelper.ShowSuccess($"已成功为 {totalAffectedCount} 个变量添加/更新MQTT服务器: {selectedMqtt.Name} 的别名。");
            }
            else
            {
                NotificationHelper.ShowInfo($"没有新的变量关联或别名更新到MQTT服务器: {selectedMqtt.Name}。");
            }
        }
        catch (Exception ex)
        {
            // 捕获并显示错误通知
            NotificationHelper.ShowError($"添加MQTT服务器失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 修改选定变量的启用状态。
    /// </summary>
    /// <param name="variablesToChange">要修改启用状态的变量数据列表。</param>
    [RelayCommand]
    public async Task ModifyIsActive(IList<object> variablesToChange)
    {
        var validVariables = variablesToChange?.OfType<VariableData>()
                                              .ToList();

        if (validVariables == null || !validVariables.Any())
        {
            NotificationHelper.ShowInfo("请选择要修改启用状态的变量");
            return;
        }

        // 假设所有选中的变量都应该被设置为相同的状态，取第一个变量的当前状态的反值
        var currentIsActive = validVariables.First()
                                            .IsActive;
        var newIsActive = !currentIsActive;

        var confirm = await _dialogService.ShowIsActiveDialog(newIsActive);

        if (confirm.HasValue && confirm.Value == newIsActive)
        {
            foreach (var variable in validVariables)
            {
                variable.IsActive = newIsActive;
            }

            await _varDataRepository.UpdateAsync(validVariables);

            // 更新界面
            await RefreshDataView();


            NotificationHelper.ShowSuccess($"已成功将 {validVariables.Count} 个变量的启用状态修改为 {newIsActive}");
        }
        else
        {
            NotificationHelper.ShowInfo("操作已取消或状态未改变。");
        }
    }

    /// <summary>
    /// 当变量表的启用/禁用状态改变时调用。
    /// 更新数据库中变量表的激活状态，并显示相应的通知。
    /// </summary>
    /// <param name="active">变量表的新激活状态（true为启用，false为禁用）。</param>
    public async Task OnIsActiveChanged(bool active)
    {
        // 更新数据库中变量表的激活状态
        var res = await _varTableRepository.UpdateAsync(VariableTable);
        if (res > 0)
        {
            // 根据激活状态显示成功通知
            var statusMessage = active ? "已启用" : "已停用";
            NotificationHelper.ShowSuccess($"变量表：{VariableTable.Name},{statusMessage}");
        }
        else
        {
            // 显示失败通知
            NotificationHelper.ShowError($"变量表：{VariableTable.Name},状态修改失败，状态：{active}");
            // _logger.LogInformation($"变量表：{VariableTable.Name},状态修改失败，状态：{active}"); // 可以选择记录日志
        }
    }
}