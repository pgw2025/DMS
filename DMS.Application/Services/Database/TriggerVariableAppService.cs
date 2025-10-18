using AutoMapper;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Core.Interfaces;
using DMS.Core.Models.Triggers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Application.Services.Database;

/// <summary>
/// ITriggerVariableAppService 的实现，负责管理触发器与变量的关联关系。
/// </summary>
public class TriggerVariableAppService : ITriggerVariableAppService
{
    private readonly IRepositoryManager _repositoryManager;

    /// <summary>
    /// 构造函数。
    /// </summary>
    public TriggerVariableAppService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    /// <summary>
    /// 异步为触发器分配或更新一个变量关联。
    /// </summary>
    public async Task<TriggerVariable> AssignTriggerVariableAsync(TriggerVariable triggerVariable)
    {
        return await _repositoryManager.TriggerVariables.AddAsync(triggerVariable);
    }

    /// <summary>
    /// 异步更新一个已存在的触发器与变量关联。
    /// </summary>
    public async Task<int> UpdateTriggerVariableAsync(TriggerVariable triggerVariable)
    {
        return await _repositoryManager.TriggerVariables.UpdateAsync(triggerVariable);
    }

    /// <summary>
    /// 异步移除一个触发器与变量关联。
    /// </summary>
    public async Task<int> RemoveTriggerVariableAsync(int triggerVariableId)
    {
        return await _repositoryManager.TriggerVariables.DeleteByIdAsync(triggerVariableId);
    }

    public async Task<List<TriggerVariable>> GetAllAsync()
    {
        var triggerVariables = await _repositoryManager.TriggerVariables.GetAllAsync();
       
        return triggerVariables;
    }
    
    
    /// <summary>
    /// 批量添加触发器与变量的关联关系
    /// </summary>
    public async Task<List<TriggerVariable>> AddTriggerVariablesAsync(List<TriggerVariable> triggerVariables)
    {
        return await _repositoryManager.TriggerVariables.AddBatchAsync(triggerVariables);
    }
    
}