using DMS.Core.Models.Triggers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Application.Interfaces.Management;

public interface ITriggerVariableManagementService
{
    Task<TriggerVariable> AssignTriggerVariableAsync(TriggerVariable triggerVariable);
    Task<int> UpdateAsync(TriggerVariable triggerVariable);
    Task<bool> DeleteAsync(int id);
    Task<List<TriggerVariable>> LoadAllTriggerVariablesAsync();
    
    // /// <summary>
    // /// 根据触发器ID获取关联的变量ID列表
    // /// </summary>
    // /// <param name="triggerId">触发器ID</param>
    // /// <returns>变量ID列表</returns>
    // Task<List<int>> GetVariableIdsByTriggerIdAsync(int triggerId);
    //
    // /// <summary>
    // /// 根据变量ID获取关联的触发器ID列表
    // /// </summary>
    // /// <param name="variableId">变量ID</param>
    // /// <returns>触发器ID列表</returns>
    // Task<List<int>> GetTriggerIdsByVariableIdAsync(int variableId);
    
    /// <summary>
    /// 批量添加触发器与变量的关联关系
    /// </summary>
    /// <param name="triggerVariables">触发器与变量的关联列表</param>
    /// <returns>异步操作任务</returns>
    Task<List<TriggerVariable>> AddTriggerVariablesAsync(List<TriggerVariable> triggerVariables);
    
    /// <summary>
    /// 根据触发器ID删除关联关系
    /// </summary>
    /// <param name="triggerId">触发器ID</param>
    /// <returns>异步操作任务</returns>
    Task<bool> DeleteByTriggerIdAsync(int triggerId);
}