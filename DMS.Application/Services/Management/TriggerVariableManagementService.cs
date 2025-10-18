using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;
using DMS.Core.Models.Triggers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Application.Services.Management;

public class TriggerVariableManagementService : ITriggerVariableManagementService
{
    private readonly ITriggerVariableAppService _triggerVariableAppService;
    private readonly IAppStorageService _appStorageService;
    private readonly IEventService _eventService;

    public TriggerVariableManagementService(ITriggerVariableAppService triggerVariableAppService,IAppStorageService appStorageService,IEventService eventService)
    {
        _triggerVariableAppService = triggerVariableAppService;
        _appStorageService = appStorageService;
        _eventService = eventService;
    }

    public async Task<TriggerVariable> AssignTriggerVariableAsync(TriggerVariable triggerVariable)
    {
        var newTriggerVariable = await _triggerVariableAppService.AssignTriggerVariableAsync(triggerVariable);
        if (newTriggerVariable != null)
        {
            // Add to cache
            _appStorageService.TriggerVariables.TryAdd(newTriggerVariable.Id, newTriggerVariable);

            _eventService.RaiseTriggerVariableChanged(this, new TriggerVariableChangedEventArgs(ActionChangeType.Added, newTriggerVariable));
        }

        return newTriggerVariable;
    }

    public async Task<List<TriggerVariable>> LoadAllTriggerVariablesAsync()
    {
        var triggerVariables = await _triggerVariableAppService.GetAllAsync();
        foreach (var triggerVariable in triggerVariables)
        {
            // Add to cache
            _appStorageService.TriggerVariables.TryAdd(triggerVariable.Id, triggerVariable);


            if (_appStorageService.Triggers.TryGetValue(triggerVariable.TriggerDefinitionId, out var trigger))
            {
                if (_appStorageService.Variables.TryGetValue(triggerVariable.VariableId, out var variable))
                {
                    trigger.Variables.Add(variable);
                    variable.Triggers.Add(trigger);
                    
                }    
            }
            
            
            _eventService.RaiseTriggerVariableChanged(this, new TriggerVariableChangedEventArgs(ActionChangeType.Added, triggerVariable));
        }

        return triggerVariables;
    }

    public async Task<int> UpdateAsync(TriggerVariable triggerVariable)
    {
        int res = await _triggerVariableAppService.UpdateTriggerVariableAsync(triggerVariable);
        if (res > 0)
        {
            // Update cache
            if (_appStorageService.TriggerVariables.TryGetValue(triggerVariable.Id, out var existingTriggerVariable))
            {
                existingTriggerVariable.TriggerDefinitionId = triggerVariable.TriggerDefinitionId;
                existingTriggerVariable.VariableId = triggerVariable.VariableId;
            }
        }
        return res;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _triggerVariableAppService.RemoveTriggerVariableAsync(id);
        if (result == 0) return false;

        if (_appStorageService.TriggerVariables.TryGetValue(id, out var triggerVariable))
        {
            _appStorageService.TriggerVariables.TryRemove(triggerVariable.Id, out _);
            _eventService.RaiseTriggerVariableChanged(
                this, new TriggerVariableChangedEventArgs(ActionChangeType.Deleted, triggerVariable));
        }
        return true;
    }
    
    // public async Task<List<int>> GetVariableIdsByTriggerIdAsync(int triggerId)
    // {
    //     return await _triggerVariableAppService.GetVariableIdsByTriggerIdAsync(triggerId);
    // }
    
    // public async Task<List<int>> GetTriggerIdsByVariableIdAsync(int variableId)
    // {
    //     return await _triggerVariableAppService.GetTriggerIdsByVariableIdAsync(variableId);
    // }
    
    public async Task<List<TriggerVariable>> AddTriggerVariablesAsync(List<TriggerVariable> triggerVariables)
    {
        var addedTriggerVariables = await _triggerVariableAppService.AddTriggerVariablesAsync(triggerVariables);
        foreach (var triggerVariable in addedTriggerVariables)
        {
            // Add to cache
            _appStorageService.TriggerVariables.TryAdd(triggerVariable.Id, triggerVariable);
            _eventService.RaiseTriggerVariableChanged(this, new TriggerVariableChangedEventArgs(ActionChangeType.Added, triggerVariable));
        }

        return addedTriggerVariables;
    }
    
    public async Task<bool> DeleteByTriggerIdAsync(int triggerId)
    {
        // var result = await _triggerVariableAppService.RemoveTriggerVariablesByTriggerIdAsync(triggerId);
        // 注意：这里可能需要额外的缓存管理逻辑，因为删除的是多个条目
        // 可能需要根据triggerId获取这些变量ID并从缓存中移除
        // 为简化实现，我们先不处理缓存中的逐个删除，而依赖于后续的重新加载
        // return result != null;
        return false;
    }
}