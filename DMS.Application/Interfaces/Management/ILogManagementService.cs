using DMS.Application.DTOs;
using DMS.Application.Events;

namespace DMS.Application.Interfaces.Management;

public interface ILogManagementService
{
    /// <summary>
    /// 异步根据ID获取日志DTO。
    /// </summary>
    Task<NlogDto> GetNlogByIdAsync(int id);

    /// <summary>
    /// 异步获取所有日志DTO列表。
    /// </summary>
    Task<List<NlogDto>> GetAllNlogsAsync();

    /// <summary>
    /// 异步获取指定数量的最新日志DTO列表。
    /// </summary>
    Task<List<NlogDto>> GetLatestNlogsAsync(int count);

    /// <summary>
    /// 异步清空所有日志。
    /// </summary>
    Task ClearAllNlogsAsync();

    /// <summary>
    /// 在内存中添加日志
    /// </summary>
    void AddNlogToMemory(NlogDto nlogDto);

    /// <summary>
    /// 在内存中更新日志
    /// </summary>
    void UpdateNlogInMemory(NlogDto nlogDto);

    /// <summary>
    /// 在内存中删除日志
    /// </summary>
    void RemoveNlogFromMemory(int nlogId);

    /// <summary>
    /// 当日志数据发生变化时触发
    /// </summary>
    event EventHandler<NlogChangedEventArgs> OnLogChanged;

    /// <summary>
    /// 异步加载所有日志数据到内存中。
    /// </summary>
    Task LoadAllNlogsAsync(int count);
}