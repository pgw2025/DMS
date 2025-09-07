using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories;

/// <summary>
/// Nlog日志仓储接口，定义了对Nlog实体的特定数据访问方法。
/// 继承自通用仓储接口 IBaseRepository<Nlog>。
/// </summary>
public interface INlogRepository : IBaseRepository<Nlog>
{
    /// <summary>
    /// 异步删除所有Nlog日志。
    /// </summary>
    Task DeleteAllAsync();

    // 可以在此处添加 Nlog 特定的查询方法，例如：
    // Task<List<Nlog>> GetLogsByLevelAsync(string level);
    // Task<List<Nlog>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate);
    // Task<List<Nlog>> SearchLogsAsync(string searchTerm);
}