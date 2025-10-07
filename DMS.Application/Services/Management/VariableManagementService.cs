using System.Collections.Concurrent;
using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Application.Services.Management;

/// <summary>
/// 变量管理服务，负责变量相关的业务逻辑。
/// </summary>
public class VariableManagementService : IVariableManagementService
{
    private readonly IVariableAppService _variableAppService;
    private readonly IEventService _eventService;
    private readonly IMapper _mapper;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly IDataProcessingService _dataProcessingService;


    public VariableManagementService(IVariableAppService variableAppService,
                                     IEventService eventService,
                                     IMapper mapper,
                                     IAppDataStorageService appDataStorageService,
                                     IDataProcessingService dataProcessingService)
    {
        _variableAppService = variableAppService;
        _eventService = eventService;
        _mapper = mapper;
        _appDataStorageService = appDataStorageService;
        _dataProcessingService = dataProcessingService;
    }

    /// <summary>
    /// 异步根据ID获取变量DTO。
    /// </summary>
    public async Task<Variable> GetVariableByIdAsync(int id)
    {
        return await _variableAppService.GetVariableByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有变量DTO列表。
    /// </summary>
    public async Task<List<Variable>> GetAllVariablesAsync()
    {
        return await _variableAppService.GetAllVariablesAsync();
    }

    /// <summary>
    /// 异步创建一个新变量。
    /// </summary>
    public async Task<Variable> CreateVariableAsync(Variable variable)
    {
        var result = await _variableAppService.CreateVariableAsync(variable);
        
        // 创建成功后，将变量添加到内存中
        if (result != null)
        {
            if (_appDataStorageService.VariableTables.TryGetValue(result.VariableTableId, out var variableTable))
            {
                result.VariableTable = variableTable;
                variableTable.Variables.Add(result);
            }

            if (_appDataStorageService.Variables.TryAdd(result.Id, result))
            {
                _eventService.RaiseVariableChanged(
                    this, new VariableChangedEventArgs(ActionChangeType.Added, result));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步更新一个已存在的变量。
    /// </summary>
    public async Task<int> UpdateVariableAsync(Variable variable)
    {
        return await UpdateVariablesAsync(new List<Variable>() { variable});
    }

    /// <summary>
    /// 异步批量更新变量。
    /// </summary>
    public async Task<int> UpdateVariablesAsync(List<Variable> variables)
    {
        var result = await _variableAppService.UpdateVariablesAsync(variables);
        
        // 批量更新成功后，更新内存中的变量
        if (result > 0 && variables != null)
        {
            foreach (var variable in variables)
            {
                if (_appDataStorageService.Variables.TryGetValue(variable.Id, out var mVariable))
                {
                    // 比较旧值和新值，确定哪个属性发生了变化
                    var changedProperties = GetChangedProperties(mVariable, variable);
                    
                    // 更新内存中的变量
                    _mapper.Map(variable, mVariable);

                    // 为每个发生变化的属性触发事件
                    foreach (var property in changedProperties)
                    {
                        _eventService.RaiseVariableChanged(
                            this, new VariableChangedEventArgs(ActionChangeType.Updated, variable, property));
                    }
                    
                    // 如果没有任何属性发生变化，至少触发一次更新事件
                    if (changedProperties.Count == 0)
                    {
                        _eventService.RaiseVariableChanged(
                            this, new VariableChangedEventArgs(ActionChangeType.Updated, variable, VariablePropertyType.All));
                    }
                }
                else
                {
                    // 如果内存中不存在该变量，则直接添加
                    _appDataStorageService.Variables.TryAdd(variable.Id, variable);
                    _eventService.RaiseVariableChanged(
                        this, new VariableChangedEventArgs(ActionChangeType.Added, variable, VariablePropertyType.All));
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步删除一个变量。
    /// </summary>
    public async Task<bool> DeleteVariableAsync(int id)
    {
        var result = await _variableAppService.DeleteVariableAsync(id);
        
        // 删除成功后，从内存中移除变量
        if (result)
        {
            if (_appDataStorageService.Variables.TryRemove(id, out var variable))
            {
                if (variable != null && _appDataStorageService.VariableTables.TryGetValue(variable.VariableTableId, out var variableTable))
                {
                    variableTable.Variables.Remove(variable);
                   
                }

                _eventService.RaiseVariableChanged(
                   this, new VariableChangedEventArgs(ActionChangeType.Deleted, variable));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步批量导入变量。
    /// </summary>
    public async Task<List<Variable>> BatchImportVariablesAsync(List<Variable> variables)
    {
        var result = await _variableAppService.BatchImportVariablesAsync(variables);
        foreach (var variable in result)
        {
            if (_appDataStorageService.VariableTables.TryGetValue(variable.VariableTableId ,out var variableTable))
            {
                variable.VariableTable = variableTable;
            }
            
        }
        
        
        // 批量导入成功后，触发批量导入事件
        if (result != null && result.Any())
        {
            _eventService.RaiseBatchImportVariables(this, new BatchImportVariablesEventArgs(result));
        }
        
        return result;
    }

    public async Task<List<Variable>> FindExistingVariablesAsync(IEnumerable<Variable> variablesToCheck)
    {
        return await _variableAppService.FindExistingVariablesAsync(variablesToCheck);
    }

    /// <summary>
    /// 获取发生变化的属性列表
    /// </summary>
    /// <param name="oldVariable">旧变量值</param>
    /// <param name="newVariable">新变量值</param>
    /// <returns>发生变化的属性列表</returns>
    private List<VariablePropertyType> GetChangedProperties(Variable oldVariable, Variable newVariable)
    {
        var changedProperties = new List<VariablePropertyType>();

        if (oldVariable.Name != newVariable.Name)
            changedProperties.Add(VariablePropertyType.Name);
        
        if (oldVariable.S7Address != newVariable.S7Address)
            changedProperties.Add(VariablePropertyType.S7Address);
        
        if (oldVariable.DataType != newVariable.DataType)
            changedProperties.Add(VariablePropertyType.DataType);
        
        if (oldVariable.ConversionFormula != newVariable.ConversionFormula)
            changedProperties.Add(VariablePropertyType.ConversionFormula);
        
        if (oldVariable.OpcUaUpdateType != newVariable.OpcUaUpdateType)
            changedProperties.Add(VariablePropertyType.OpcUaUpdateType);
        
        if (oldVariable.MqttAliases != newVariable.MqttAliases)
            changedProperties.Add(VariablePropertyType.MqttAlias);
        
        if (oldVariable.Description != newVariable.Description)
            changedProperties.Add(VariablePropertyType.Description);
        
        if (oldVariable.VariableTableId != newVariable.VariableTableId)
            changedProperties.Add(VariablePropertyType.VariableTableId);

        if (oldVariable.DataValue != newVariable.DataValue)
            changedProperties.Add(VariablePropertyType.Value);
            
        if (oldVariable.IsActive != newVariable.IsActive)
            changedProperties.Add(VariablePropertyType.IsActive);
        if (oldVariable.IsHistoryEnabled != newVariable.IsHistoryEnabled)
            changedProperties.Add(VariablePropertyType.IsHistoryEnabled);
            
        if (oldVariable.OpcUaNodeId != newVariable.OpcUaNodeId)
            changedProperties.Add(VariablePropertyType.OpcUaNodeId);
            
        if (oldVariable.PollingInterval != newVariable.PollingInterval)
            changedProperties.Add(VariablePropertyType.PollingInterval);
            
        if (oldVariable.SignalType != newVariable.SignalType)
            changedProperties.Add(VariablePropertyType.SignalType);
            
        if (oldVariable.Protocol != newVariable.Protocol)
            changedProperties.Add(VariablePropertyType.Protocol);

        return changedProperties;
    }


    /// <summary>
    /// 异步批量删除变量。
    /// </summary>
    public async Task<bool> DeleteVariablesAsync(List<int> ids)
    {
        var result = await _variableAppService.DeleteVariablesAsync(ids);
        
        // 批量删除成功后，从内存中移除变量
        if (result && ids != null)
        {
            foreach (var id in ids)
            {
                if (_appDataStorageService.Variables.TryRemove(id, out var variable))
                {
                    if (variable != null && _appDataStorageService.VariableTables.TryGetValue(variable.VariableTableId, out var variableTable))
                    {
                        variableTable.Variables.Remove(variable);
                    }

                    _eventService.RaiseVariableChanged(
                        this, new VariableChangedEventArgs(ActionChangeType.Deleted, variable));
                }
            }
        }
        
        return result;
    }

    
}