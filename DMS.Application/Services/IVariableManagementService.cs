using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Application.DTOs.Events;
using DMS.Core.Events;
using DMS.Core.Models;

namespace DMS.Application.Services;

public interface IVariableManagementService
{
    /// <summary>
    /// 异步根据ID获取变量DTO。
    /// </summary>
    Task<VariableDto> GetVariableByIdAsync(int id);

    /// <summary>
    /// 异步获取所有变量DTO列表。
    /// </summary>
    Task<List<VariableDto>> GetAllVariablesAsync();

    /// <summary>
    /// 异步创建一个新变量。
    /// </summary>
    Task<VariableDto> CreateVariableAsync(VariableDto variableDto);

    /// <summary>
    /// 异步更新一个已存在的变量。
    /// </summary>
    Task<int> UpdateVariableAsync(VariableDto variableDto);

    /// <summary>
    /// 异步批量更新变量。
    /// </summary>
    Task<int> UpdateVariablesAsync(List<VariableDto> variableDtos);

    /// <summary>
    /// 异步删除一个变量。
    /// </summary>
    Task<bool> DeleteVariableAsync(int id);

    /// <summary>
    /// 异步批量删除变量。
    /// </summary>
    Task<bool> DeleteVariablesAsync(List<int> ids);

    /// <summary>
    /// 在内存中添加变量
    /// </summary>
    void AddVariableToMemory(VariableDto variableDto, ConcurrentDictionary<int, VariableTableDto> variableTables);

    /// <summary>
    /// 在内存中更新变量
    /// </summary>
    void UpdateVariableInMemory(VariableDto variableDto, ConcurrentDictionary<int, VariableTableDto> variableTables);

    /// <summary>
    /// 在内存中删除变量
    /// </summary>
    void RemoveVariableFromMemory(int variableId, ConcurrentDictionary<int, VariableTableDto> variableTables);
}