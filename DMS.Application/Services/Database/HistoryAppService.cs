using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces.Database;
using DMS.Core.Interfaces;

namespace DMS.Application.Services.Database;

/// <summary>
/// 历史记录应用服务实现类，负责处理变量历史记录相关的业务逻辑。
/// </summary>
public class HistoryAppService : IHistoryAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入仓储管理器和AutoMapper。
    /// </summary>
    /// <param name="repoManager">仓储管理器实例。</param>
    /// <param name="mapper">AutoMapper实例。</param>
    public HistoryAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步获取指定变量的历史记录。
    /// </summary>
    /// <param name="variableId">变量ID</param>
    /// <returns>变量历史记录列表</returns>
    public async Task<List<VariableHistoryDto>> GetVariableHistoriesAsync(int variableId)
    {
        var histories = await _repoManager.VariableHistories.GetByVariableIdAsync(variableId);
        return _mapper.Map<List<VariableHistoryDto>>(histories);
    }
    
    /// <summary>
    /// 异步获取指定变量的历史记录，支持条数限制和时间范围筛选。
    /// </summary>
    /// <param name="variableId">变量ID</param>
    /// <param name="limit">返回记录的最大数量，null表示无限制</param>
    /// <param name="startTime">开始时间，null表示无限制</param>
    /// <param name="endTime">结束时间，null表示无限制</param>
    /// <returns>变量历史记录列表</returns>
    public async Task<List<VariableHistoryDto>> GetVariableHistoriesAsync(int variableId, int? limit = null, DateTime? startTime = null, DateTime? endTime = null)
    {
        var histories = await _repoManager.VariableHistories.GetByVariableIdAsync(variableId, limit, startTime, endTime);
        return _mapper.Map<List<VariableHistoryDto>>(histories);
    }
    
    /// <summary>
    /// 异步获取所有变量的历史记录。
    /// </summary>
    /// <returns>所有变量历史记录列表</returns>
    public async Task<List<VariableHistoryDto>> GetAllVariableHistoriesAsync()
    {
        var histories = await _repoManager.VariableHistories.GetAllAsync();
        return _mapper.Map<List<VariableHistoryDto>>(histories);
    }
    
    /// <summary>
    /// 异步获取所有变量的历史记录，支持条数限制和时间范围筛选。
    /// </summary>
    /// <param name="limit">返回记录的最大数量，null表示无限制</param>
    /// <param name="startTime">开始时间，null表示无限制</param>
    /// <param name="endTime">结束时间，null表示无限制</param>
    /// <returns>所有变量历史记录列表</returns>
    public async Task<List<VariableHistoryDto>> GetAllVariableHistoriesAsync(int? limit = null, DateTime? startTime = null, DateTime? endTime = null)
    {
        var histories = await _repoManager.VariableHistories.GetAllAsync(limit, startTime, endTime);
        return _mapper.Map<List<VariableHistoryDto>>(histories);
    }
}