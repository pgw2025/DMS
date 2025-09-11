using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories;

public interface IVariableHistoryRepository:IBaseRepository<VariableHistory>
{
    /// <summary>
    /// 根据变量ID获取历史记录
    /// </summary>
    /// <param name="variableId">变量ID</param>
    /// <returns>变量历史记录列表</returns>
    Task<List<VariableHistory>> GetByVariableIdAsync(int variableId);
    
    /// <summary>
    /// 根据变量ID获取历史记录，支持条数限制和时间范围筛选
    /// </summary>
    /// <param name="variableId">变量ID</param>
    /// <param name="limit">返回记录的最大数量，null表示无限制</param>
    /// <param name="startTime">开始时间，null表示无限制</param>
    /// <param name="endTime">结束时间，null表示无限制</param>
    /// <returns>变量历史记录列表</returns>
    Task<List<VariableHistory>> GetByVariableIdAsync(int variableId, int? limit = null, DateTime? startTime = null, DateTime? endTime = null);
    
    /// <summary>
    /// 获取所有历史记录，支持条数限制和时间范围筛选
    /// </summary>
    /// <param name="limit">返回记录的最大数量，null表示无限制</param>
    /// <param name="startTime">开始时间，null表示无限制</param>
    /// <param name="endTime">结束时间，null表示无限制</param>
    /// <returns>所有历史记录列表</returns>
    Task<List<VariableHistory>> GetAllAsync(int? limit = null, DateTime? startTime = null, DateTime? endTime = null);
}