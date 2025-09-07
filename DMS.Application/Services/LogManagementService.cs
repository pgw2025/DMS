using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Core.Models;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Core.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace DMS.Application.Services;

/// <summary>
/// 日志管理服务，负责日志相关的业务逻辑。
/// </summary>
public class LogManagementService
{
    private readonly INlogAppService _nlogAppService;
    private readonly ConcurrentDictionary<int, NlogDto> _nlogs;

    /// <summary>
    /// 当日志数据发生变化时触发
    /// </summary>
    public event EventHandler<NlogChangedEventArgs> NlogChanged;

    public LogManagementService(INlogAppService nlogAppService, 
                               ConcurrentDictionary<int, NlogDto> nlogs)
    {
        _nlogAppService = nlogAppService;
        _nlogs = nlogs;
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
        if (_nlogs.TryAdd(nlogDto.Id, nlogDto))
        {
            OnNlogChanged(new NlogChangedEventArgs(DataChangeType.Added, nlogDto));
        }
    }

    /// <summary>
    /// 在内存中更新日志
    /// </summary>
    public void UpdateNlogInMemory(NlogDto nlogDto)
    {
        _nlogs.AddOrUpdate(nlogDto.Id, nlogDto, (key, oldValue) => nlogDto);
        OnNlogChanged(new NlogChangedEventArgs(DataChangeType.Updated, nlogDto));
    }

    /// <summary>
    /// 在内存中删除日志
    /// </summary>
    public void RemoveNlogFromMemory(int nlogId)
    {
        if (_nlogs.TryRemove(nlogId, out var nlogDto))
        {
            OnNlogChanged(new NlogChangedEventArgs(DataChangeType.Deleted, nlogDto));
        }
    }

    /// <summary>
    /// 触发日志变更事件
    /// </summary>
    protected virtual void OnNlogChanged(NlogChangedEventArgs e)
    {
        NlogChanged?.Invoke(this, e);
    }
}