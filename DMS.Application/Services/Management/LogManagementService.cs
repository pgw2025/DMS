using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;

namespace DMS.Application.Services.Management;

/// <summary>
/// 日志管理服务，负责日志相关的业务逻辑。
/// </summary>
public class LogManagementService : ILogManagementService
{
    private readonly INlogAppService _nlogAppService;
    private readonly IAppStorageService _appStorageService;

    /// <summary>
    /// 当日志数据发生变化时触发
    /// </summary>
    public event EventHandler<NlogChangedEventArgs> OnLogChanged;

    public LogManagementService(INlogAppService nlogAppService,IAppStorageService appStorageService)
    {
        _nlogAppService = nlogAppService;
        _appStorageService = appStorageService;
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
        if (_appStorageService.Nlogs.TryAdd(nlogDto.Id, nlogDto))
        {
            OnLogChanged?.Invoke(this,new NlogChangedEventArgs(DataChangeType.Added, nlogDto));
        }
    }

    /// <summary>
    /// 在内存中更新日志
    /// </summary>
    public void UpdateNlogInMemory(NlogDto nlogDto)
    {
        _appStorageService.Nlogs.AddOrUpdate(nlogDto.Id, nlogDto, (key, oldValue) => nlogDto);
        OnLogChanged?.Invoke(this,new NlogChangedEventArgs(DataChangeType.Updated, nlogDto));
    }

    /// <summary>
    /// 在内存中删除日志
    /// </summary>
    public void RemoveNlogFromMemory(int nlogId)
    {
        if (_appStorageService.Nlogs.TryRemove(nlogId, out var nlogDto))
        {
            OnLogChanged?.Invoke(this,new NlogChangedEventArgs(DataChangeType.Deleted, nlogDto));
        }
    }

    /// <summary>
    /// 异步加载所有日志数据到内存中。
    /// </summary>
    public async Task LoadAllNlogsAsync(int count)
    {
        _appStorageService.Nlogs.Clear();
        var nlogDtos = await _nlogAppService.GetLatestLogsAsync(count);
        // 加载日志数据到内存
        foreach (var nlogDto in nlogDtos)
        {
            _appStorageService.Nlogs.TryAdd(nlogDto.Id, nlogDto);
        }
    }

}