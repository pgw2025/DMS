using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Models;
using DMS.WPF.Interfaces;
using DMS.WPF.ViewModels.Items;
using ObservableCollections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.WPF.ViewModels.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System;

namespace DMS.WPF.ViewModels;

partial class LogHistoryViewModel : ViewModelBase
{
    private readonly IMapper _mapper;
    private readonly INlogAppService _nlogAppService;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private NlogItemViewModel _selectedLog;

    [ObservableProperty]
    private IList _selectedLogs = new ArrayList();

    [ObservableProperty]
    private string _searchText;

    [ObservableProperty]
    private string _selectedLogLevel;

    private readonly ObservableList<NlogItemViewModel> _logItemList;
    private readonly ISynchronizedView<NlogItemViewModel, NlogItemViewModel> _synchronizedView;
    public NotifyCollectionChangedSynchronizedViewList<NlogItemViewModel> LogItemListView { get; }

    public ObservableCollection<string> LogLevels { get; } = new ObservableCollection<string> { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" };

    public LogHistoryViewModel(IMapper mapper, INlogAppService nlogAppService, IDialogService dialogService, INotificationService notificationService)
    {
        _mapper = mapper;
        _nlogAppService = nlogAppService;
        _dialogService = dialogService;
        _notificationService = notificationService;

        _logItemList = new ObservableList<NlogItemViewModel>();
        _synchronizedView = _logItemList.CreateView(v => v);
        LogItemListView = _synchronizedView.ToNotifyCollectionChanged();
    }

    private bool FilterLogs(NlogItemViewModel item)
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
                return new NlogItemViewModel(nlog);
            }).ToList();
            
            _logItemList.Clear();
            _logItemList.AddRange(logItems);
        }
        catch (System.Exception ex)
        {
            _notificationService.ShowError($"加载日志时发生错误: {ex.Message}", ex);
        }
    }
}