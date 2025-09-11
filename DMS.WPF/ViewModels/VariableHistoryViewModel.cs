using System.Collections;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;

namespace DMS.WPF.ViewModels;

partial class VariableHistoryViewModel : ViewModelBase,INavigatable
{
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly IVariableAppService _variableAppService;
    private readonly IWPFDataService _wpfDataService;
    private readonly IDataStorageService _dataStorageService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// 当前选中的设备
    /// </summary>
    [ObservableProperty]
    private DeviceItemViewModel _selectedDevice;

    /// <summary>
    /// 当前选中的变量表
    /// </summary>
    [ObservableProperty]
    private VariableTableItemViewModel _selectedVariableTable;

    /// <summary>
    /// 当前选中的变量
    /// </summary>
    [ObservableProperty]
    private VariableItemViewModel _selectedVariable;

    /// <summary>
    /// 用于过滤变量的搜索文本
    /// </summary>
    [ObservableProperty]
    private string _searchText;

    /// <summary>
    /// 所有设备列表
    /// </summary>
    public NotifyCollectionChangedSynchronizedViewList<DeviceItemViewModel> Devices { get; }

    /// <summary>
    /// 当前设备下的变量表列表
    /// </summary>
    public NotifyCollectionChangedSynchronizedViewList<VariableTableItemViewModel> VariableTables { get; }

    /// <summary>
    /// 当前变量表下的变量列表
    /// </summary>
    public NotifyCollectionChangedSynchronizedViewList<VariableItemViewModel> Variables { get; }

    /// <summary>
    /// 变量历史记录列表
    /// </summary>
    public NotifyCollectionChangedSynchronizedViewList<VariableHistoryDto> VariableHistories { get; }

    private readonly ObservableList<DeviceItemViewModel> _deviceItemList;
    private readonly ObservableList<VariableTableItemViewModel> _variableTableItemList;
    private readonly ObservableList<VariableItemViewModel> _variableItemList;
    private readonly ObservableList<VariableHistoryDto> _variableHistoryList;

    private readonly ISynchronizedView<DeviceItemViewModel, DeviceItemViewModel> _deviceSynchronizedView;
    private readonly ISynchronizedView<VariableTableItemViewModel, VariableTableItemViewModel> _variableTableSynchronizedView;
    private readonly ISynchronizedView<VariableItemViewModel, VariableItemViewModel> _variableSynchronizedView;
    private readonly ISynchronizedView<VariableHistoryDto, VariableHistoryDto> _variableHistorySynchronizedView;

    public VariableHistoryViewModel(IMapper mapper, IDialogService dialogService, IVariableAppService variableAppService,
                                  IWPFDataService wpfDataService, IDataStorageService dataStorageService, INotificationService notificationService)
    {
        _mapper = mapper;
        _dialogService = dialogService;
        _variableAppService = variableAppService;
        _wpfDataService = wpfDataService;
        _dataStorageService = dataStorageService;
        _notificationService = notificationService;

        _deviceItemList = new ObservableList<DeviceItemViewModel>();
        _variableTableItemList = new ObservableList<VariableTableItemViewModel>();
        _variableItemList = new ObservableList<VariableItemViewModel>();
        _variableHistoryList = new ObservableList<VariableHistoryDto>();

        _deviceSynchronizedView = _deviceItemList.CreateView(v => v);
        _variableTableSynchronizedView = _variableTableItemList.CreateView(v => v);
        _variableSynchronizedView = _variableItemList.CreateView(v => v);
        _variableHistorySynchronizedView = _variableHistoryList.CreateView(v => v);

        Devices = _deviceSynchronizedView.ToNotifyCollectionChanged();
        VariableTables = _variableTableSynchronizedView.ToNotifyCollectionChanged();
        Variables = _variableSynchronizedView.ToNotifyCollectionChanged();
        VariableHistories = _variableHistorySynchronizedView.ToNotifyCollectionChanged();
    }



    /// <summary>
    /// 加载所有设备
    /// </summary>
    private void LoadDevices()
    {
        _deviceItemList.Clear();
        _deviceItemList.AddRange(_dataStorageService.Devices);
    }

    /// <summary>
    /// 当选中的设备发生变化时
    /// </summary>
    /// <param name="value"></param>
    partial void OnSelectedDeviceChanged(DeviceItemViewModel value)
    {
        if (value != null)
        {
            // 清空变量表和变量列表
            _variableTableItemList.Clear();
            _variableItemList.Clear();
            _variableHistoryList.Clear();

            // 加载选中设备下的变量表
            _variableTableItemList.AddRange(value.VariableTables);
            
            // 清空选中项
            SelectedVariableTable = null;
            SelectedVariable = null;
        }
    }

    /// <summary>
    /// 当选中的变量表发生变化时
    /// </summary>
    /// <param name="value"></param>
    partial void OnSelectedVariableTableChanged(VariableTableItemViewModel value)
    {
        if (value != null)
        {
            // 清空变量列表和历史记录
            _variableItemList.Clear();
            _variableHistoryList.Clear();

            // 加载选中变量表下的变量
            _variableItemList.AddRange(value.Variables);
            
            // 清空选中项
            SelectedVariable = null;
        }
    }

    /// <summary>
    /// 当选中的变量发生变化时
    /// </summary>
    /// <param name="value"></param>
    partial void OnSelectedVariableChanged(VariableItemViewModel value)
    {
        // if (value != null)
        // {
        //     // 加载选中变量的历史记录
        //     LoadVariableHistories(value.Id);
        // }
        // else
        // {
        //     _variableHistoryList.Clear();
        // }
    }

    /// <summary>
    /// 加载变量的历史记录
    /// </summary>
    /// <param name="variableId"></param>
    private async void LoadVariableHistories(int variableId)
    {
        try
        {
            _variableHistoryList.Clear();
            var histories = await _variableAppService.GetVariableHistoriesAsync(variableId);
            _variableHistoryList.AddRange(histories);
        }
        catch (Exception ex)
        {
            // 记录更详细的错误信息
            _notificationService.ShowError($"加载变量历史记录失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 搜索变量
    /// </summary>
    /// <param name="value"></param>
    partial void OnSearchTextChanged(string value)
    {
        if (SelectedVariableTable == null) return;

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            _variableSynchronizedView.ResetFilter();
        }
        else
        {
            _variableSynchronizedView.AttachFilter(FilterVariables);
        }
    }

    /// <summary>
    /// 过滤变量
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    private bool FilterVariables(VariableItemViewModel item)
    {
        var searchTextLower = SearchText.ToLower();
        return item.Name?.ToLower().Contains(searchTextLower) == true ||
               item.Description?.ToLower().Contains(searchTextLower) == true ||
               item.OpcUaNodeId?.ToLower().Contains(searchTextLower) == true ||
               item.S7Address?.ToLower().Contains(searchTextLower) == true;
    }

    public async Task OnNavigatedToAsync(MenuItemViewModel menu)
    {
        
        VariableItemViewModel variable =_dataStorageService.Variables.FirstOrDefault(v => v.Id == menu.TargetId);
        if (variable!=null)
        {
            // 直接设置选中的变量
            SelectedVariable = variable;

            
            // 加载历史记录
            LoadVariableHistories(variable.Id);
        }
    }
}