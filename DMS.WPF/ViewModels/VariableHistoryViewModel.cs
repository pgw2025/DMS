using System.Windows.Media;
using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Core.Enums;
using DMS.Core.Events;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.ItemViewModel;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.WPF;
using ObservableCollections;
using SkiaSharp;

namespace DMS.WPF.ViewModels;

partial class VariableHistoryViewModel : ViewModelBase, INavigatable
{
    private readonly IMapper _mapper;
    private readonly IDialogService _dialogService;
    private readonly IHistoryAppService _historyAppService;
    private readonly IWPFDataService _wpfDataService;
    private readonly IWpfDataService _dataStorageService;
    private readonly IEventService _eventService;
    private readonly INotificationService _notificationService;
    private readonly INavigationService _navigationService;

    /// <summary>
    /// 加载历史记录条数限制
    /// </summary>
    [ObservableProperty]
    private int? _initHistoryLimit;
    
    /// <summary>
    /// 显示历史记录条数限制
    /// </summary>
    [ObservableProperty]
    private int? _viewHistoryLimit;

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
    private VariableItem _currentVariable;

    /// <summary>
    /// 变量历史记录列表
    /// </summary>
    public NotifyCollectionChangedSynchronizedViewList<VariableHistoryDto> VariableHistories { get; }

    // 折线图相关属性
    public ISeries[] LineSeriesCollection { get; set; }

    [ObservableProperty]
    private Axis[] _lineAxisX;

    [ObservableProperty]
    private Axis[] _lineAxisY;

    private readonly ObservableList<VariableHistoryDto> _variableHistoryList;
    private readonly ISynchronizedView<VariableHistoryDto, VariableHistoryDto> _variableHistorySynchronizedView;

    /// <summary>
    /// 所有变量的缓存列表，用于搜索
    /// </summary>
    private List<VariableHistoryDto> _allVariableHistories;

    public VariableHistoryViewModel(IMapper mapper, IDialogService dialogService, IHistoryAppService historyAppService,
                                    IWPFDataService wpfDataService, IWpfDataService dataStorageService,
                                    IEventService eventService, INotificationService notificationService,
                                    INavigationService navigationService)
    {
        _mapper = mapper;
        _dialogService = dialogService;
        _historyAppService = historyAppService;
        _wpfDataService = wpfDataService;
        _dataStorageService = dataStorageService;
        _eventService = eventService;
        _notificationService = notificationService;
        _navigationService = navigationService;

        _variableHistoryList = new ObservableList<VariableHistoryDto>();
        _variableHistorySynchronizedView = _variableHistoryList.CreateView(v => v);
        VariableHistories = _variableHistorySynchronizedView.ToNotifyCollectionChanged();
        _allVariableHistories = new List<VariableHistoryDto>();

        // 初始化默认值
        _initHistoryLimit = 50; // 默认限制1000条记录
        _startTime = null;
        _endTime = null;

        // 初始化图表属性
        LineAxisX = new Axis[] { new Axis() };
        LineAxisY = new Axis[] { new Axis() };
        LineSeriesCollection = new ISeries[0];

        _eventService.OnVariableValueChanged += OnVariableValueChanged;
    }

    private void OnVariableValueChanged(object? sender, VariableValueChangedEventArgs e)
    {
        if (CurrentVariable is null)
        {
            return;
        }

        if (e.Variable.Id != CurrentVariable.Id)
        {
            return;
        }

        var variableHistory = new VariableHistoryDto()
                              {
                                  VariableId = CurrentVariable.Id,
                                  Timestamp = DateTime.Now,
                                  Value = e.Variable.DisplayValue,
                                  NumericValue = e.Variable.NumericValue
                              };
        _variableHistoryList.Add(variableHistory);

        // 限制历史记录数量以防止内存溢出
        if (ViewHistoryLimit.HasValue && _variableHistoryList.Count > ViewHistoryLimit.Value)
        {
            // 移除最旧的记录
            _variableHistoryList.RemoveAt(0);
        }

        // 更新图表数据 - 使用更高效的方法处理实时更新
        UpdateChartData();
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
            _allVariableHistories = allHistories.OrderBy(v => v.Timestamp)
                                                .ToList();
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


    public override async Task OnNavigatedToAsync(NavigationParameter parameter)
    {
        if (_dataStorageService.Variables.TryGetValue(parameter.TargetId, out VariableItem variableItem))
        {
            CurrentVariable = variableItem;
            // 加载所有变量的历史记录
            LoadAllVariableHistories(variableItem.Id, InitHistoryLimit, StartTime, EndTime);
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
            LoadAllVariableHistories(CurrentVariable.Id, InitHistoryLimit, StartTime, EndTime);
        }
    }

    /// <summary>
    /// 返回变量表命令
    /// </summary>
    [RelayCommand]
    private async Task NavigateToVariableTable()
    {
        try
        {
            // 导航到变量表页面
            await _navigationService.NavigateToAsync(this,new NavigationParameter(nameof(VariableTableViewModel),CurrentVariable.VariableTableId,NavigationType.VariableTable));
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"导航到变量表失败: {ex.Message}", ex);
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

    // 添加字段来保存轴实例，以保持缩放状态
    private Axis[] _lineAxisXInstance;
    private Axis[] _lineAxisYInstance;


    /// <summary>
    /// 更新图表数据
    /// </summary>
    private void UpdateChartData()
    {
        if (_variableHistoryList == null || _variableHistoryList.Count == 0)
        {
            // 清空图表数据
            LineSeriesCollection = new ISeries[0];
            OnPropertyChanged(nameof(LineSeriesCollection));
            return;
        }

        // 如果系列集合为空或没有有效的系列，则重新创建
        if (LineSeriesCollection == null || LineSeriesCollection.Length == 0)
        {
            // 创建数值点集合
            var values = new List<DateTimePoint>();

            foreach (var history in _variableHistoryList)
            {
                values.Add(new DateTimePoint(history.Timestamp, history.NumericValue));
            }

            // 创建线性序列
            var series = new LineSeries<DateTimePoint>
                         {
                             Name = CurrentVariable?.Name ?? "变量值",
                             Values = values,
                             Fill = null,
                             Stroke = new SolidColorPaint(new SKColor(41, 128, 185)) { StrokeThickness = 2 },
                             GeometrySize = 6, // 显示数据点，圆点大小为6
                             LineSmoothness = 5 // 使用直线连接点，也可以设为其他值实现曲线
                         };

            // 更新序列集合
            LineSeriesCollection = new ISeries[] { series };

            // 初始化坐标轴并保存实例引用
            _lineAxisXInstance = new Axis[]
                                 {
                                     new Axis
                                     {
                                         Labeler = value => new DateTime((long)value).ToString("MM-dd HH:mm:ss"),
                                         // 不设置固定范围，让图表根据数据自动调整
                                     }
                                 };

            _lineAxisYInstance = new Axis[]
                                 {
                                     new Axis
                                     {
                                         Name = CurrentVariable?.Name ?? "值",
                                         MinLimit = 0 // 设置Y轴从0开始
                                     }
                                 };

            // 设置属性值
            LineAxisX = _lineAxisXInstance;
            LineAxisY = _lineAxisYInstance;

            // 通知属性更改
            OnPropertyChanged(nameof(LineSeriesCollection));
            OnPropertyChanged(nameof(LineAxisX));
            OnPropertyChanged(nameof(LineAxisY));
        }
        else
        {
            // 对于实时更新，保持原有完整的更新逻辑以确保数据一致性
            // 创建数值点集合
            var values = new List<DateTimePoint>();

            foreach (var history in _variableHistoryList)
            {
                // 尝试将值转换为double
                values.Add(new DateTimePoint(history.Timestamp, history.NumericValue));
            }

            // 更新当前系列
            var currentSeries = (LineSeries<DateTimePoint>)LineSeriesCollection[0];
            currentSeries.Values = values;
            currentSeries.Name = CurrentVariable?.Name ?? "变量值";

            // 通知系列更改，但保留当前轴的缩放状态（不需要更新轴）
            // OnPropertyChanged(nameof(LineSeriesCollection));
        }
    }
}