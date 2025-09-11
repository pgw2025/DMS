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
}