using DMS.Core.Models.Triggers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Application.Interfaces.Database;

/// <summary>
/// 定义了触发器与变量关联管理相关的应用服务操作。
/// </summary>
public interface ITriggerVariableAppService
{

    /// <summary>
    /// 异步为触发器分配或更新一个变量关联。
    /// </summary>
    /// <param name="triggerVariable"></param>
    Task<TriggerVariable> AssignTriggerVariableAsync(TriggerVariable triggerVariable);

    /// <summary>
    /// 异步更新一个已存在的触发器与变量关联。
    /// </summary>
    /// <param name="triggerVariable">要更新的触发器与变量关联对象。</param>
    Task<int> UpdateTriggerVariableAsync(TriggerVariable triggerVariable);

    /// <summary>
    /// 异步移除一个触发器与变量关联。
    /// </summary>
    /// <param name="triggerVariableId">要移除的关联的ID。</param>
    Task<int> RemoveTriggerVariableAsync(int triggerVariableId);
    
    /// <summary>
    /// 异步获取所有触发器与变量关联。
    /// </summary>
    Task<List<TriggerVariable>> GetAllAsync();
    
    /// <summary>
    /// 批量添加触发器与变量的关联关系
    /// </summary>
    /// <param name="triggerVariables">触发器与变量的关联列表</param>
    /// <returns>异步操作任务</returns>
    Task<List<TriggerVariable>> AddTriggerVariablesAsync(List<TriggerVariable> triggerVariables);
    
}