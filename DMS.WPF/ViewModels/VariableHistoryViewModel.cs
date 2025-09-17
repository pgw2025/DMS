using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Core.Events;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using ObservableCollections;

namespace DMS.WPF.ViewModels;

partial class VariableHistoryViewModel : ViewModelBase, INavigatable
{
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly IHistoryAppService _historyAppService;
    private readonly IWPFDataService _wpfDataService;
    private readonly IDataStorageService _dataStorageService;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;

    /// <summary>
    /// 历史记录条数限制
    /// </summary>
    [ObservableProperty]
    private int? _historyLimit;

    /// <summary>
    /// 历史记录开始时间
    /// </summary>
    [ObservableProperty]
    private DateTime? _startTime;

    /// <summary>
    /// 历史记录结束时间
    /// </summary>
    [ObservableProperty]
    private DateTime? _endTime;

    /// <summary>
    /// 选中的变量历史记录
    /// </summary>
    [ObservableProperty]
    private VariableItemViewModel _currentVariable;

    /// <summary>
    /// 变量历史记录列表
    /// </summary>
    public NotifyCollectionChangedSynchronizedViewList<VariableHistoryDto> VariableHistories { get; }

    // 折线图相关属性
    public SeriesCollection LineSeriesCollection { get; set; }
    
    [ObservableProperty] 
    private AxesCollection _lineAxisX;
    
    [ObservableProperty] 
    private AxesCollection _lineAxisY;

    private readonly ObservableList<VariableHistoryDto> _variableHistoryList;
    private readonly ISynchronizedView<VariableHistoryDto, VariableHistoryDto> _variableHistorySynchronizedView;

    /// <summary>
    /// 所有变量的缓存列表，用于搜索
    /// </summary>
    private List<VariableHistoryDto> _allVariableHistories;

    public VariableHistoryViewModel(IMapper mapper, IDialogService dialogService, IHistoryAppService historyAppService,
                                    IWPFDataService wpfDataService, IDataStorageService dataStorageService,
                                    IEventService eventService, 
                                    INotificationService notificationService)
    {
        _mapper = mapper;
        _dialogService = dialogService;
        _historyAppService = historyAppService;
        _wpfDataService = wpfDataService;
        _dataStorageService = dataStorageService;
        _eventService = eventService;
        _notificationService = notificationService;

        _variableHistoryList = new ObservableList<VariableHistoryDto>();
        _variableHistorySynchronizedView = _variableHistoryList.CreateView(v => v);
        VariableHistories = _variableHistorySynchronizedView.ToNotifyCollectionChanged();
        _allVariableHistories = new List<VariableHistoryDto>();

        // 初始化默认值
        _historyLimit = 1000; // 默认限制1000条记录
        _startTime = null;
        _endTime = null;
        
        // 初始化图表属性
        LineAxisX = new AxesCollection { new Axis() };
        LineAxisY = new AxesCollection { new Axis() };
        LineSeriesCollection = new SeriesCollection();

        _eventService.OnVariableValueChanged += OnVariableValueChanged;
    }

    private void OnVariableValueChanged(object? sender, VariableValueChangedEventArgs e)
    {
        if (e.VariableId!= CurrentVariable.Id)
        {
            return;
        }
        
        var variableHistory = new VariableHistoryDto()
                              {
                                  VariableId = CurrentVariable.Id,
                                  Timestamp = DateTime.Now,
                                  Value = e.NewValue
                              };
        _variableHistoryList.Add(variableHistory);
        
    }

    /// <summary>
    /// 加载所有变量的历史记录
    /// </summary>
    /// <param name="limit">返回记录的最大数量，null表示无限制</param>
    /// <param name="startTime">开始时间，null表示无限制</param>
    /// <param name="endTime">结束时间，null表示无限制</param>
    private async void LoadAllVariableHistories(int variableId, int? limit = null, DateTime? startTime = null,
                                                DateTime? endTime = null)
    {
        try
        {
            _variableHistoryList.Clear();
            var allHistories
                = await _historyAppService.GetVariableHistoriesAsync(variableId, limit, startTime, endTime);
            _allVariableHistories = allHistories.ToList();
            _variableHistoryList.AddRange(_allVariableHistories);

            // 更新图表数据
            UpdateChartData();
        }
        catch (Exception ex)
        {
            // 记录更详细的错误信息
            _notificationService.ShowError($"加载变量历史记录失败: {ex.Message}", ex);
        }
    }


    public async Task OnNavigatedToAsync(MenuItemViewModel menu)
    {
        if (_dataStorageService.Variables.TryGetValue(menu.TargetId, out VariableItemViewModel variableItem))
        {
            CurrentVariable = variableItem;
            // 加载所有变量的历史记录
            LoadAllVariableHistories(variableItem.Id, HistoryLimit, StartTime, EndTime);
        }
    }

    /// <summary>
    /// 重新加载历史记录命令
    /// </summary>
    [RelayCommand]
    private void Reload()
    {
        if (CurrentVariable != null)
        {
            LoadAllVariableHistories(CurrentVariable.Id, HistoryLimit, StartTime, EndTime);
        }
    }


    /// <summary>
    /// 根据搜索文本过滤历史记录
    /// </summary>
    /// <param name="searchText"></param>
    private void FilterHistoriesBySearchText(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            // 如果搜索文本为空，显示所有历史记录
            _variableHistoryList.Clear();
            _variableHistoryList.AddRange(_allVariableHistories);
        }
        else
        {
            // 根据搜索文本过滤历史记录
            var filteredHistories = _allVariableHistories
                                    .Where(h =>
                                               h.VariableName?.Contains(
                                                   searchText, StringComparison.OrdinalIgnoreCase) == true)
                                    .ToList();

            _variableHistoryList.Clear();
            _variableHistoryList.AddRange(filteredHistories);
        }
    }

    /// <summary>
    /// 根据变量ID加载历史记录
    /// </summary>
    /// <param name="variableId">变量ID</param>
    /// <param name="limit">返回记录的最大数量，null表示无限制</param>
    /// <param name="startTime">开始时间，null表示无限制</param>
    /// <param name="endTime">结束时间，null表示无限制</param>
    public async Task LoadVariableHistoriesAsync(int variableId, int? limit = null, DateTime? startTime = null,
                                                 DateTime? endTime = null)
    {
        try
        {
            _variableHistoryList.Clear();
            var histories = await _historyAppService.GetVariableHistoriesAsync(variableId, limit, startTime, endTime);
            _variableHistoryList.AddRange(histories);

            // 更新图表数据
            UpdateChartData();
        }
        catch (Exception ex)
        {
            // 记录更详细的错误信息
            _notificationService.ShowError($"加载变量历史记录失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 更新图表数据
    /// </summary>
    private void UpdateChartData()
    {
        if (_variableHistoryList == null || _variableHistoryList.Count == 0)
        {
            // 清空图表数据
            LineSeriesCollection?.Clear();
            OnPropertyChanged(nameof(LineSeriesCollection));
            return;
        }

        // 创建数值点集合
        var values = new ChartValues<DateTimePoint>();

        foreach (var history in _variableHistoryList)
        {
            // 尝试将值转换为double
            if (double.TryParse(history.Value, out double value))
            {
                values.Add(new DateTimePoint(history.Timestamp, value));
            }
        }

        // 确保LineSeriesCollection已初始化
        if (LineSeriesCollection == null)
        {
            LineSeriesCollection = new SeriesCollection();
        }

        // 清空旧数据并添加新系列
        LineSeriesCollection.Clear();
        LineSeriesCollection.Add(new LineSeries
        {
            Title = "变量值",
            Values = values,
            PointGeometry = null, // 不显示数据点，只显示线条
            LineSmoothness = 0.5, // 设置线条平滑度
            StrokeThickness = 2
        });

        // 确保坐标轴集合已初始化
        if (LineAxisX == null || LineAxisX.Count == 0)
        {
            LineAxisX = new AxesCollection { new Axis() };
        }
        if (LineAxisY == null || LineAxisY.Count == 0)
        {
            LineAxisY = new AxesCollection { new Axis() };
        }

        // 设置坐标轴
        // X轴使用时间格式化
        LineAxisX[0].LabelFormatter = value => new DateTime((long)value).ToString("MM-dd HH:mm:ss");

        // Y轴使用数值格式化
        LineAxisY[0].Title = "值";
        LineAxisY[0].LabelFormatter = value => value.ToString("F2");

        // 通知属性更改
        OnPropertyChanged(nameof(LineSeriesCollection));
        OnPropertyChanged(nameof(LineAxisX));
        OnPropertyChanged(nameof(LineAxisY));
    }
}