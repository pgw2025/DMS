using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;

namespace DMS.Application.Services;

/// <summary>
/// Nlog日志应用服务，负责处理Nlog日志相关的业务逻辑。
/// 实现 <see cref="INlogAppService"/> 接口。
/// </summary>
public class NlogAppService : INlogAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，通过依赖注入获取仓储管理器和AutoMapper实例。
    /// </summary>
    /// <param name="repoManager">仓储管理器实例。</param>
    /// <param name="mapper">AutoMapper 实例。</param>
    public NlogAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取Nlog日志数据传输对象。
    /// </summary>
    /// <param name="id">日志ID。</param>
    /// <returns>Nlog日志数据传输对象。</returns>
    public async Task<NlogDto> GetLogByIdAsync(int id)
    {
        var log = await _repoManager.Nlogs.GetByIdAsync(id);
        return _mapper.Map<NlogDto>(log);
    }

    /// <summary>
    /// 异步获取所有Nlog日志数据传输对象列表。
    /// </summary>
    /// <returns>Nlog日志数据传输对象列表。</returns>
    public async Task<List<NlogDto>> GetAllLogsAsync()
    {
        var logs = await _repoManager.Nlogs.GetAllAsync();
        return _mapper.Map<List<NlogDto>>(logs);
    }

    /// <summary>
    /// 异步获取指定数量的最新Nlog日志数据传输对象列表。
    /// </summary>
    /// <param name="count">要获取的日志条目数量。</param>
    /// <returns>最新的Nlog日志数据传输对象列表。</returns>
    public async Task<List<NlogDto>> GetLatestLogsAsync(int count)
    {
        // 注意：这里的实现假设仓储层或数据库支持按时间倒序排列并取前N条。
        // 如果 BaseRepository 没有提供这种能力，可能需要直接访问 DbNlog 实体。
        // 例如：var dbLogs = await _repoManager.Nlogs.Db.Queryable<Infrastructure.Entities.DbNlog>().OrderByDescending(n => n.LogTime).Take(count).ToListAsync();
        // var logs = _mapper.Map<List<Core.Models.Nlog>>(dbLogs);
        // return _mapper.Map<List<NlogDto>>(logs);

        // 为简化起见，这里先调用 GetAll 然后在内存中排序和截取（仅适用于日志量不大的情况）。
        // 生产环境中建议优化数据库查询。
        var allLogs = await GetAllLogsAsync();
        return allLogs.OrderByDescending(l => l.LogTime).Take(count).ToList();
    }

    // 可以在这里实现 INlogAppService 接口中定义的其他方法
    // 例如：
    /*
    public async Task<List<NlogDto>> GetLogsByLevelAsync(string level)
    {
        // 假设 INlogRepository 有 GetLogsByLevelAsync 方法
        var logs = await _repoManager.Nlogs.GetLogsByLevelAsync(level);
        return _mapper.Map<List<NlogDto>>(logs);
    }

    public async Task<List<NlogDto>> GetLogsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var logs = await _repoManager.Nlogs.GetLogsByDateRangeAsync(startDate, endDate);
        return _mapper.Map<List<NlogDto>>(logs);
    }

    public async Task<List<NlogDto>> SearchLogsAsync(string searchTerm)
    {
        var logs = await _repoManager.Nlogs.SearchLogsAsync(searchTerm);
        return _mapper.Map<List<NlogDto>>(logs);
    }
    */
}