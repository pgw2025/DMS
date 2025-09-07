using DMS.Application.DTOs;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义Nlog日志管理相关的应用服务操作。
/// </summary>
public interface INlogAppService
{
    /// <summary>
    /// 异步根据ID获取Nlog日志DTO。
    /// </summary>
    Task<NlogDto> GetLogByIdAsync(int id);

    /// <summary>
    /// 异步获取所有Nlog日志DTO列表。
    /// </summary>
    Task<List<NlogDto>> GetAllLogsAsync();

    /// <summary>
    /// 异步获取指定数量的最新Nlog日志DTO列表。
    /// </summary>
    /// <param name="count">要获取的日志条目数量。</param>
    Task<List<NlogDto>> GetLatestLogsAsync(int count);

    /// <summary>
    /// 异步清空所有Nlog日志。
    /// </summary>
    Task ClearAllLogsAsync();

    // 可以在这里添加更多针对日志的查询服务方法，例如：
    // Task<List<NlogDto>> GetLogsByLevelAsync(string level);
    // Task<List<NlogDto>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
    // Task<List<NlogDto>> SearchLogsAsync(string searchTerm);
}