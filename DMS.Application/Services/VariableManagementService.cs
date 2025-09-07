using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Core.Models;
using DMS.Application.Interfaces;
using DMS.Core.Interfaces;
using DMS.Core.Enums;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace DMS.Application.Services;

/// <summary>
/// 变量管理服务，负责变量相关的业务逻辑。
/// </summary>
public class VariableManagementService
{
    private readonly IVariableAppService _variableAppService;
    private readonly ConcurrentDictionary<int, VariableDto> _variables;

    /// <summary>
    /// 当变量数据发生变化时触发
    /// </summary>
    public event EventHandler<VariableChangedEventArgs> VariableChanged;

    public VariableManagementService(IVariableAppService variableAppService, ConcurrentDictionary<int, VariableDto> variables)
    {
        _variableAppService = variableAppService;
        _variables = variables;
    }

    /// <summary>
    /// 异步根据ID获取变量DTO。
    /// </summary>
    public async Task<VariableDto> GetVariableByIdAsync(int id)
    {
        return await _variableAppService.GetVariableByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有变量DTO列表。
    /// </summary>
    public async Task<List<VariableDto>> GetAllVariablesAsync()
    {
        return await _variableAppService.GetAllVariablesAsync();
    }

    /// <summary>
    /// 异步创建一个新变量。
    /// </summary>
    public async Task<VariableDto> CreateVariableAsync(VariableDto variableDto)
    {
        return await _variableAppService.CreateVariableAsync(variableDto);
    }

    /// <summary>
    /// 异步更新一个已存在的变量。
    /// </summary>
    public async Task<int> UpdateVariableAsync(VariableDto variableDto)
    {
        return await _variableAppService.UpdateVariableAsync(variableDto);
    }

    /// <summary>
    /// 异步批量更新变量。
    /// </summary>
    public async Task<int> UpdateVariablesAsync(List<VariableDto> variableDtos)
    {
        return await _variableAppService.UpdateVariablesAsync(variableDtos);
    }

    /// <summary>
    /// 异步删除一个变量。
    /// </summary>
    public async Task<bool> DeleteVariableAsync(int id)
    {
        return await _variableAppService.DeleteVariableAsync(id);
    }

    /// <summary>
    /// 异步批量删除变量。
    /// </summary>
    public async Task<bool> DeleteVariablesAsync(List<int> ids)
    {
        return await _variableAppService.DeleteVariablesAsync(ids);
    }

    /// <summary>
    /// 在内存中添加变量
    /// </summary>
    public void AddVariableToMemory(VariableDto variableDto, ConcurrentDictionary<int, VariableTableDto> variableTables)
    {
        VariableTableDto variableTableDto = null;
        if (variableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
        {
            variableTableDto = variableTable;
            variableDto.VariableTable = variableTableDto;
            variableTable.Variables.Add(variableDto);
        }

        if (_variables.TryAdd(variableDto.Id, variableDto))
        {
            OnVariableChanged(new VariableChangedEventArgs(DataChangeType.Added, variableDto, variableTableDto));
        }
    }

    /// <summary>
    /// 在内存中更新变量
    /// </summary>
    public void UpdateVariableInMemory(VariableDto variableDto, ConcurrentDictionary<int, VariableTableDto> variableTables)
    {
        VariableTableDto variableTableDto = null;
        if (variableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
        {
            variableTableDto = variableTable;
        }

        _variables.AddOrUpdate(variableDto.Id, variableDto, (key, oldValue) => variableDto);
        OnVariableChanged(new VariableChangedEventArgs(DataChangeType.Updated, variableDto, variableTableDto));
    }

    /// <summary>
    /// 在内存中删除变量
    /// </summary>
    public void RemoveVariableFromMemory(int variableId, ConcurrentDictionary<int, VariableTableDto> variableTables)
    {
        if (_variables.TryRemove(variableId, out var variableDto))
        {
            VariableTableDto variableTableDto = null;
            if (variableDto != null && variableTables.TryGetValue(variableDto.VariableTableId, out var variableTable))
            {
                variableTableDto = variableTable;
                variableTable.Variables.Remove(variableDto);
            }

            OnVariableChanged(new VariableChangedEventArgs(DataChangeType.Deleted, variableDto, variableTableDto));
        }
    }

    /// <summary>
    /// 触发变量变更事件
    /// </summary>
    protected virtual void OnVariableChanged(VariableChangedEventArgs e)
    {
        VariableChanged?.Invoke(this, e);
    }
}