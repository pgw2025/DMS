using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Application.Interfaces;

namespace DMS.Application.Services;

/// <summary>
/// 日志管理服务，负责日志相关的业务逻辑。
/// </summary>
public class LogManagementService : ILogManagementService
{
    private readonly INlogAppService _nlogAppService;
    private readonly IAppDataStorageService _appDataStorageService;

    /// <summary>
    /// 当日志数据发生变化时触发
    /// </summary>
    public event EventHandler<NlogChangedEventArgs> OnLogChanged;

    public LogManagementService(INlogAppService nlogAppService,IAppDataStorageService appDataStorageService)
    {
        _nlogAppService = nlogAppService;
        _appDataStorageService = appDataStorageService;
    }

    /// <summary>
    /// 异步根据ID获取日志DTO。
    /// </summary>
    public async Task<NlogDto> GetNlogByIdAsync(int id)
    {
        return await _nlogAppService.GetLogByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有日志DTO列表。
    /// </summary>
    public async Task<List<NlogDto>> GetAllNlogsAsync()
    {
        return await _nlogAppService.GetAllLogsAsync();
    }

    /// <summary>
    /// 异步获取指定数量的最新日志DTO列表。
    /// </summary>
    public async Task<List<NlogDto>> GetLatestNlogsAsync(int count)
    {
        return await _nlogAppService.GetLatestLogsAsync(count);
    }

    /// <summary>
    /// 异步清空所有日志。
    /// </summary>
    public async Task ClearAllNlogsAsync()
    {
        await _nlogAppService.ClearAllLogsAsync();
    }

    /// <summary>
    /// 在内存中添加日志
    /// </summary>
    public void AddNlogToMemory(NlogDto nlogDto)
    {
        if (_appDataStorageService.Nlogs.TryAdd(nlogDto.Id, nlogDto))
        {
            OnLogChanged?.Invoke(this,new NlogChangedEventArgs(DataChangeType.Added, nlogDto));
        }
    }

    /// <summary>
    /// 在内存中更新日志
    /// </summary>
    public void UpdateNlogInMemory(NlogDto nlogDto)
    {
        _appDataStorageService.Nlogs.AddOrUpdate(nlogDto.Id, nlogDto, (key, oldValue) => nlogDto);
        OnLogChanged?.Invoke(this,new NlogChangedEventArgs(DataChangeType.Updated, nlogDto));
    }

    /// <summary>
    /// 在内存中删除日志
    /// </summary>
    public void RemoveNlogFromMemory(int nlogId)
    {
        if (_appDataStorageService.Nlogs.TryRemove(nlogId, out var nlogDto))
        {
            OnLogChanged?.Invoke(this,new NlogChangedEventArgs(DataChangeType.Deleted, nlogDto));
        }
    }


}