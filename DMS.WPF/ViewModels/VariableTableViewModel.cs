using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Enums;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.Services;
using DMS.WPF.ViewModels.Dialogs;
using DMS.WPF.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    /// 当前选中的变量数据。
    /// 通过 ObservableProperty 自动生成 SelectedVariable 属性和 OnSelectedVariableDataChanged 方法。
    /// </summary>
    [ObservableProperty]
    private VariableItemViewModel _selectedVariable;


    [ObservableProperty]
    private IList _selectedVariables = new ArrayList();

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

    private readonly INotificationService _notificationService;

    public VariableTableViewModel(IMapper mapper, IDialogService dialogService, IVariableAppService variableAppService,
                                  DataServices dataServices, INotificationService notificationService)
    {
        _mapper = mapper;
        _dialogService = dialogService;
        _variableAppService = variableAppService;
        _dataServices = dataServices;
        _notificationService = notificationService;
        IsLoadCompletion = false; // 初始设置为 false，表示未完成加载


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
    }


    /// <summary>
    /// 编辑选定的变量数据。
    /// 此命令通常绑定到UI中的“编辑”按钮或双击事件。
    /// </summary>
    /// <param name="variableTable">当前操作的变量表，用于更新其内部的变量数据。</param>
    [RelayCommand]
    private async void UpdateVariable()
    {
        try
        {
            // 检查是否有变量被选中
            if (SelectedVariable == null)
            {
                _notificationService.ShowInfo("请选择要编辑的变量");
                return;
            }

            // 显示编辑变量数据的对话框，并传入当前选中的变量数据
            VariableDialogViewModel variableDialogViewModel
                = App.Current.Services.GetRequiredService<VariableDialogViewModel>();
            variableDialogViewModel.Title = "编辑变量";
            variableDialogViewModel.PrimaryButText = "保存修改";
            variableDialogViewModel.IsAddModel = false;
            // 创建一个副本用于编辑，避免直接修改原数据
            var variableToEdit = new VariableItemViewModel();
            _mapper.Map(SelectedVariable, variableToEdit);
            variableDialogViewModel.Variable = variableToEdit;

            var editedVariable = await _dialogService.ShowDialogAsync(variableDialogViewModel);

            // 如果用户取消或对话框未返回数据，则直接返回
            if (editedVariable == null)
                return;

            // 更新时间戳
            editedVariable.UpdatedAt = DateTime.Now;

            // 更新数据库中的变量数据
            var updateResult = await _variableAppService.UpdateVariableAsync(_mapper.Map<VariableDto>(editedVariable));


            if (updateResult > 0)
            {
                // 更新当前页面显示的数据：找到原数据在集合中的索引并替换
                _mapper.Map(editedVariable, SelectedVariable); // 更新选中项的属性
                // 显示成功通知
                _notificationService.ShowSuccess($"编辑变量成功:{editedVariable.Name}");
            }
            else
            {
                _notificationService.ShowError("编辑变量失败");
            }
        }
        catch (Exception e)
        {
            // 捕获并显示错误通知
            _notificationService.ShowError($"编辑变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
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
                    = new ConfirmDialogViewModel("存在已经添加的变量", $"变量名称:{existNames}，已经存在，是否跳过继续添加其他的变量。取消则不添加任何变量", "继续");
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
                    _notificationService.ShowSuccess($"从Excel导入变量成功，共导入变量：{improtVariableDtos.Count}个");
                }
            }
            else
            {
                _notificationService.ShowSuccess($"列表中没有要添加的变量了。 ");
            }
        }
        catch (Exception e)
        {
            _notificationService.ShowError($"从TIA导入变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 从OPC UA服务器导入变量数据。
    /// 此命令通常绑定到UI中的“从OPC UA导入”按钮。
    /// </summary>
    [RelayCommand]
    private async void ImportFromOpcUaServer()
    {
        try
        {
            if (CurrentVariableTable.Device == null)
            {
                _notificationService.ShowError("当前变量表的Device对象为空，请检查。");
                return;
            }

            // 检查OPC UA Endpoint URL是否已设置
            string opcUaEndpointUrl = CurrentVariableTable.Device.OpcUaServerUrl;
            if (string.IsNullOrEmpty(opcUaEndpointUrl))
            {
                _notificationService.ShowError("OPC UA Endpoint URL 未设置。请在设备详情中配置。");
                return;
            }

            // 显示OPC UA导入对话框，让用户选择要导入的变量
            ImportOpcUaDialogViewModel importOpcUaDialogViewModel
                = App.Current.Services.GetRequiredService<ImportOpcUaDialogViewModel>();
            importOpcUaDialogViewModel.EndpointUrl = opcUaEndpointUrl; // 设置Endpoint URL
            var importedVariables = await _dialogService.ShowDialogAsync(importOpcUaDialogViewModel);
            if (importedVariables == null || !importedVariables.Any())
            {
                return; // 用户取消或没有选择任何变量
            }

            // 将导入的变量转换为DTO并设置必要的属性
            var importedVariableDtos = _mapper.Map<List<VariableDto>>(importedVariables);
            foreach (var variableDto in importedVariableDtos)
            {
                variableDto.CreatedAt = DateTime.Now;
                variableDto.UpdatedAt = DateTime.Now;
                variableDto.VariableTableId = CurrentVariableTable.Id;
            }

            // 检查是否存在同名变量
            var existList = await _variableAppService.FindExistingVariablesAsync(importedVariableDtos);
            if (existList.Count > 0)
            {
                // 拼接要删除的变量名称，用于确认提示
                var existNames = string.Join("、", existList.Select(v => v.Name));
                var confirmDialogViewModel
                    = new ConfirmDialogViewModel("存在已经添加的变量", $"变量名称:{existNames}，已经存在，是否跳过继续添加其他的变量。取消则不添加任何变量", "继续");
                var res = await _dialogService.ShowDialogAsync(confirmDialogViewModel);
                if (!res) return;
                // 从导入列表中删除已经存在的变量
                importedVariableDtos.RemoveAll(variableDto => existList.Contains(variableDto));
            }

            // 如果还有变量需要导入，则执行导入操作
            if (importedVariableDtos.Count != 0)
            {
                var isSuccess = await _variableAppService.BatchImportVariablesAsync(importedVariableDtos);
                if (isSuccess)
                {
                    var addVariableDtos = await _variableAppService.GetVariableByOpcUaNodeIdsAsync(
                        importedVariableDtos.Select(v => v.OpcUaNodeId)
                                            .ToList());
                    _variableItemList.AddRange(_mapper.Map<List<VariableItemViewModel>>(addVariableDtos));
                    _notificationService.ShowSuccess($"从OPC UA服务器导入变量成功，共导入变量：{importedVariableDtos.Count}个");
                }
                else
                {
                    _notificationService.ShowError("从OPC UA服务器导入变量失败");
                }
            }
            else
            {
                _notificationService.ShowSuccess("列表中没有要添加的变量了。");
            }
        }
        catch (Exception e)
        {
            _notificationService.ShowError($"从OPC UA服务器导入变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }


    //
    /// <summary>
    /// 添加新的变量数据。
    /// </summary>
    /// 此命令通常绑定到UI中的“添加”按钮。
    /// <param name="variableTable">当前操作的变量表，用于设置新变量的所属ID。</param>
    [RelayCommand]
    private async void AddVariable()
    {
        try
        {
            // 显示添加变量数据的对话框
            VariableDialogViewModel variableDialogViewModel
                = App.Current.Services.GetRequiredService<VariableDialogViewModel>();
            VariableItemViewModel variableItem = new VariableItemViewModel();
            variableItem.Protocol = CurrentVariableTable.Protocol;
            variableDialogViewModel.Title = "添加变量";
            variableDialogViewModel.PrimaryButText = "添加变量";
            variableDialogViewModel.Variable = variableItem;

            var variableItemViewModel = await _dialogService.ShowDialogAsync(variableDialogViewModel);

            // 如果用户取消或对话框未返回数据，则直接返回
            if (variableItemViewModel == null)
                return;

            // // 设置新变量的所属变量表ID
            variableItemViewModel.VariableTableId = CurrentVariableTable.Id;
            variableItemViewModel.CreatedAt = DateTime.Now;
            variableItemViewModel.UpdatedAt = DateTime.Now;


            // // 添加变量数据到数据库
            var addVariable
                = await _variableAppService.CreateVariableAsync(_mapper.Map<VariableDto>(variableItemViewModel));
            _mapper.Map(addVariable, variableItemViewModel);
            // // 更新当前页面显示的数据：将新变量添加到集合中
            _variableItemList.Add(variableItemViewModel);
            _dataServices.AddVariable(variableItemViewModel);
            //
            // // 显示成功通知
            _notificationService.ShowSuccess($"添加变量成功:{variableItemViewModel.Name}");
        }
        catch (Exception e)
        {
            // 捕获并显示错误通知
            _notificationService.ShowError($"添加变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 删除选定的变量数据。
    /// 此命令通常绑定到UI中的“删除”按钮。
    /// </summary>
    /// <param name="variablesToDelete">要删除的变量数据列表。</param>
    [RelayCommand]
    public async Task DeleteVariable()
    {
        try
        {
            List<VariableItemViewModel> variablesToDelete = SelectedVariables.Cast<VariableItemViewModel>()
                                                                             .ToList();
            // 检查是否有变量被选中
            if (variablesToDelete == null || !variablesToDelete.Any())
            {
                _notificationService.ShowInfo("请选择要删除的变量");
                return;
            }

            // 拼接要删除的变量名称，用于确认提示
            var names = string.Join("、", variablesToDelete.Select(v => v.Name));

            // 显示确认删除对话框
            ConfirmDialogViewModel confirmDialogViewModel
                = new ConfirmDialogViewModel("删除变量", $"确认要删除变量：{names},删除后不可恢复，确认要删除吗？", "删除变量");
            var isDel = await _dialogService.ShowDialogAsync(confirmDialogViewModel);

            if (!isDel)
                return; // 如果用户取消删除，则返回

            // 从数据库中删除变量数据
            var result = await _variableAppService.DeleteVariablesAsync(variablesToDelete.Select(v => v.Id)
                                                                            .ToList());
            if (result)
            {
                foreach (var variable in variablesToDelete)
                {
                    _variableItemList.Remove(variable);
                    _dataServices.DeleteVariable(variable.Id);
                }

                // 显示成功通知
                _notificationService.ShowSuccess($"成功删除 {variablesToDelete.Count} 个变量");
            }
            else
            {
                // 显示删除失败通知
                _notificationService.ShowError("删除变量失败");
            }
        }
        catch (Exception e)
        {
            // 捕获并显示错误通知
            _notificationService.ShowError($"删除变量的过程中发生了不可预期的错误：{e.Message}", e);
        }
    }

    /// <summary>
    /// 更改选定变量的轮询频率。
    /// 此命令通常绑定到UI中的“修改轮询频率”按钮。
    /// </summary>
    /// <param name="variablesToChange">要修改轮询频率的变量数据列表。</param>
    [RelayCommand]
    public async Task ChangePollingInterval()
    {
        // 检查是否有变量被选中
        if (SelectedVariables.Count == 0)
        {
            _notificationService.ShowInfo("请选择要修改轮询间隔的变量");
            return;
        }

        // 获取选中的变量列表
        var validVariables = SelectedVariables.Cast<VariableItemViewModel>()
                                              .ToList();

        // 显示轮询间隔选择对话框，并传入第一个变量的当前轮询间隔作为默认值
        PollLevelDialogViewModel viewModel = new PollLevelDialogViewModel(validVariables.First()
                                                                              .PollingInterval);
        var newPollingInterval = await _dialogService.ShowDialogAsync(viewModel);
        if (newPollingInterval.HasValue)
        {
            // 更新所有选定变量的轮询间隔和修改状态
            foreach (var variable in validVariables)
            {
                variable.PollingInterval = newPollingInterval.Value;
                variable.UpdatedAt = DateTime.Now;
            }

            // 批量更新数据库中的变量数据
            var variableDtos = _mapper.Map<List<VariableDto>>(validVariables);
            var result = await _variableAppService.UpdateVariablesAsync(variableDtos);

            if (result > 0)
            {
                // 显示成功通知
                _notificationService.ShowSuccess($"已成功更新 {validVariables.Count} 个变量的轮询间隔");
            }
            else
            {
                _notificationService.ShowError("更新轮询间隔失败");
            }
        }
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
    public async Task ChangeIsActive()
    {
        // 检查是否有变量被选中
        if (SelectedVariables.Count == 0)
        {
            _notificationService.ShowInfo("请选择要修改启用状态的变量");
            return;
        }

        // 获取选中的变量列表
        var validVariables = SelectedVariables.Cast<VariableItemViewModel>()
                                              .ToList();

        // 显示启用状态选择对话框，并传入第一个变量的当前启用状态作为默认值
        IsActiveDialogViewModel viewModel = new IsActiveDialogViewModel(validVariables.First()
                                                                            .IsActive);
        var newIsActive = await _dialogService.ShowDialogAsync(viewModel);
        if (newIsActive.HasValue)
        {
            // 更新所有选定变量的启用状态和修改状态
            foreach (var variable in validVariables)
            {
                variable.IsActive = newIsActive.Value;
                variable.UpdatedAt = DateTime.Now;
            }

            // 批量更新数据库中的变量数据
            var variableDtos = _mapper.Map<List<VariableDto>>(validVariables);
            var result = await _variableAppService.UpdateVariablesAsync(variableDtos);

            if (result > 0)
            {
                // 显示成功通知
                _notificationService.ShowSuccess($"已成功更新 {validVariables.Count} 个变量的启用状态");
            }
            else
            {
                _notificationService.ShowError("更新启用状态失败");
            }
        }
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