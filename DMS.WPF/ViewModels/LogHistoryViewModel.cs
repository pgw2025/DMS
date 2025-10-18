using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using ObservableCollections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.WPF.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System;
using DMS.Application.Events;
using DMS.WPF.Services;
using DMS.Application.Interfaces.Database;
using DMS.Core.Enums;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.ViewModels;

partial class LogHistoryViewModel : ViewModelBase,IDisposable
{
    private readonly IWPFDataService _wpfDataService ;
    private readonly IMapper _mapper;
    private readonly INlogAppService _nlogAppService;
    private readonly IDialogService _dialogService;
    private readonly IDataStorageService _dataStorageService;
    private readonly INotificationService _notificationService;
    private readonly IAppCenterService _appCenterService;

    [ObservableProperty]
    private NlogItem _selectedLog;

    [ObservableProperty]
    private IList _selectedLogs = new ArrayList();

    [ObservableProperty]
    private string _searchText;

    [ObservableProperty]
    private string _selectedLogLevel;

    private readonly ObservableList<NlogItem> _logItemList;
    private readonly ISynchronizedView<NlogItem, NlogItem> _synchronizedView;
    public NotifyCollectionChangedSynchronizedViewList<NlogItem> LogItemListView { get; }

    public ObservableCollection<string> LogLevels { get; } = new ObservableCollection<string> { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" };

    public LogHistoryViewModel(IMapper mapper, INlogAppService nlogAppService, IDialogService dialogService, IDataStorageService dataStorageService
                             , INotificationService notificationService, IWPFDataService wpfDataService, IAppCenterService appCenterService)
    {
        _mapper = mapper;
        _nlogAppService = nlogAppService;
        _dialogService = dialogService;
        _dataStorageService = dataStorageService;
        _notificationService = notificationService;
        _wpfDataService = wpfDataService;
        _appCenterService = appCenterService;

        _logItemList = new ObservableList<NlogItem>(_dataStorageService.Nlogs);
        
        _synchronizedView = _logItemList.CreateView(v => v);
        LogItemListView = _synchronizedView.ToNotifyCollectionChanged();
        
        // 订阅日志变更事件
        _appCenterService.LogManagementService.OnLogChanged += _wpfDataService.LogDataService.OnNlogChanged;
    }

    /// <summary>
    /// 处理日志变更事件
    /// </summary>
    private void OnNlogChanged(object sender, NlogChangedEventArgs e)
    {
        // 在UI线程上更新日志
        App.Current.Dispatcher.BeginInvoke(new Action(() =>
        {
            switch (e.ChangeType)
            {
                case DataChangeType.Added:
                    var newLogItem = new NlogItem(new Nlog
                    {
                        Id = e.Nlog.Id,
                        LogTime = e.Nlog.LogTime,
                        Level = e.Nlog.Level,
                        ThreadId = e.Nlog.ThreadId,
                        ThreadName = e.Nlog.ThreadName,
                        Callsite = e.Nlog.Callsite,
                        CallsiteLineNumber = e.Nlog.CallsiteLineNumber,
                        Message = e.Nlog.Message,
                        Logger = e.Nlog.Logger,
                        Exception = e.Nlog.Exception,
                        CallerFilePath = e.Nlog.CallerFilePath,
                        CallerLineNumber = e.Nlog.CallerLineNumber,
                        CallerMember = e.Nlog.CallerMember
                    });
                    _logItemList.Add(newLogItem);
                    break;
                case DataChangeType.Updated:
                    var existingLog = _logItemList.FirstOrDefault(l => l.Id == e.Nlog.Id);
                    if (existingLog != null)
                    {
                        existingLog = new NlogItem(new Nlog
                        {
                            Id = e.Nlog.Id,
                            LogTime = e.Nlog.LogTime,
                            Level = e.Nlog.Level,
                            ThreadId = e.Nlog.ThreadId,
                            ThreadName = e.Nlog.ThreadName,
                            Callsite = e.Nlog.Callsite,
                            CallsiteLineNumber = e.Nlog.CallsiteLineNumber,
                            Message = e.Nlog.Message,
                            Logger = e.Nlog.Logger,
                            Exception = e.Nlog.Exception,
                            CallerFilePath = e.Nlog.CallerFilePath,
                            CallerLineNumber = e.Nlog.CallerLineNumber,
                            CallerMember = e.Nlog.CallerMember
                        });
                    }
                    break;
                case DataChangeType.Deleted:
                    var logToRemove = _logItemList.FirstOrDefault(l => l.Id == e.Nlog.Id);
                    if (logToRemove != null)
                    {
                        _logItemList.Remove(logToRemove);
                    }
                    break;
            }
        }));
    }

    private bool FilterLogs(NlogItem item)
    {
        // 搜索文本过滤
        var searchTextLower = SearchText?.ToLower() ?? string.Empty;
        var searchTextMatch = string.IsNullOrWhiteSpace(SearchText) ||
                             item.Logger?.ToLower().Contains(searchTextLower) == true ||
                             item.Message?.ToLower().Contains(searchTextLower) == true ||
                             item.Exception?.ToLower().Contains(searchTextLower) == true ||
                             item.StackTrace?.ToLower().Contains(searchTextLower) == true;

        // 日志级别过滤
        var levelMatch = string.IsNullOrWhiteSpace(SelectedLogLevel) || 
                        item.Level?.Equals(SelectedLogLevel, StringComparison.OrdinalIgnoreCase) == true;

        return searchTextMatch && levelMatch;
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedLogLevelChanged(string value)
    {
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText) && string.IsNullOrWhiteSpace(SelectedLogLevel))
        {
            _synchronizedView.ResetFilter();
        }
        else
        {
            _synchronizedView.AttachFilter(FilterLogs);
        }
    }

    public override async void OnLoaded()
    {
        await LoadLogsAsync();
    }

    [RelayCommand]
    private async Task RefreshLogsAsync()
    {
        await LoadLogsAsync();
    }

    [RelayCommand]
    private async Task ClearLogsAsync()
    {
        var confirmDialogViewModel = new ConfirmDialogViewModel("确认", "确定要清空所有日志吗？", "确定");

        var result = await _dialogService.ShowDialogAsync(confirmDialogViewModel);
        if (result == true)
        {
            try
            {
                await _nlogAppService.ClearAllLogsAsync();
                _logItemList.Clear();
                _notificationService.ShowInfo("日志已清空");
            }
            catch (System.Exception ex)
            {
                _notificationService.ShowError($"清空日志时发生错误: {ex.Message}", ex);
            }
        }
    }

    private async Task LoadLogsAsync()
    {
        try
        {
            var logs = await _nlogAppService.GetAllLogsAsync();
            
            // 按时间倒序排序
            var sortedLogs = logs.OrderByDescending(logDto => logDto.LogTime).ToList();
            
            var logItems = sortedLogs.Select(logDto => 
            {
                // Manually map NlogDto to Nlog
                var nlog = new Nlog
                {
                    Id = logDto.Id,
                    LogTime = logDto.LogTime,
                    Level = logDto.Level,
                    ThreadId = logDto.ThreadId,
                    ThreadName = logDto.ThreadName,
                    Callsite = logDto.Callsite,
                    CallsiteLineNumber = logDto.CallsiteLineNumber,
                    Message = logDto.Message,
                    Logger = logDto.Logger,
                    Exception = logDto.Exception,
                    CallerFilePath = logDto.CallerFilePath,
                    CallerLineNumber = logDto.CallerLineNumber,
                    CallerMember = logDto.CallerMember
                };
                return new NlogItem(nlog);
            }).ToList();
            
            _logItemList.Clear();
            _logItemList.AddRange(logItems);
        }
        catch (System.Exception ex)
        {
            _notificationService.ShowError($"加载日志时发生错误: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        // 取消订阅事件
        _appCenterService.LogManagementService.OnLogChanged -= OnNlogChanged;

    }
}