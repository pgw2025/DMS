using DMS.Application.DTOs;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义历史记录管理相关的应用服务操作。
/// </summary>
public interface IHistoryAppService
{
    /// <summary>
    /// 异步获取指定变量的历史记录。
    /// </summary>
    /// <param name="variableId">变量ID</param>
    /// <returns>变量历史记录列表</returns>
    Task<List<VariableHistoryDto>> GetVariableHistoriesAsync(int variableId);
    
    /// <summary>
    /// 异步获取指定变量的历史记录，支持条数限制和时间范围筛选。
    /// </summary>
    /// <param name="variableId">变量ID</param>
    /// <param name="limit">返回记录的最大数量，null表示无限制</param>
    /// <param name="startTime">开始时间，null表示无限制</param>
    /// <param name="endTime">结束时间，null表示无限制</param>
    /// <returns>变量历史记录列表</returns>
    Task<List<VariableHistoryDto>> GetVariableHistoriesAsync(int variableId, int? limit = null, DateTime? startTime = null, DateTime? endTime = null);
    
    /// <summary>
    /// 异步获取所有变量的历史记录。
    /// </summary>
    /// <returns>所有变量历史记录列表</returns>
    Task<List<VariableHistoryDto>> GetAllVariableHistoriesAsync();
    
    /// <summary>
    /// 异步获取所有变量的历史记录，支持条数限制和时间范围筛选。
    /// </summary>
    /// <param name="limit">返回记录的最大数量，null表示无限制</param>
    /// <param name="startTime">开始时间，null表示无限制</param>
    /// <param name="endTime">结束时间，null表示无限制</param>
    /// <returns>所有变量历史记录列表</returns>
    Task<List<VariableHistoryDto>> GetAllVariableHistoriesAsync(int? limit = null, DateTime? startTime = null, DateTime? endTime = null);
}