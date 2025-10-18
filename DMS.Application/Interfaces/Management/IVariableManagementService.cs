using System.Collections.Concurrent;
using DMS.Application.DTOs;
using DMS.Core.Models;

namespace DMS.Application.Interfaces.Management;

public interface IVariableManagementService
{
    /// <summary>
    /// 异步根据ID获取变量DTO。
    /// </summary>
    Task<Variable> GetVariableByIdAsync(int id);

    /// <summary>
    /// 异步获取所有变量DTO列表。
    /// </summary>
    Task<List<Variable>> GetAllVariablesAsync();

    /// <summary>
    /// 异步创建一个新变量。
    /// </summary>
    Task<Variable> CreateVariableAsync(Variable variable);

    /// <summary>
    /// 异步更新一个已存在的变量。
    /// </summary>
    Task<int> UpdateVariableAsync(Variable variable);

    /// <summary>
    /// 异步批量更新变量。
    /// </summary>
    Task<int> UpdateVariablesAsync(List<Variable> variables);

    /// <summary>
    /// 异步删除一个变量。
    /// </summary>
    Task<bool> DeleteVariableAsync(int id);

    /// <summary>
    /// 异步批量删除变量。
    /// </summary>
    Task<bool> DeleteVariablesAsync(List<int> ids);

    /// <summary>
    /// 异步批量导入变量。
    /// </summary>
    Task<List<Variable>> BatchImportVariablesAsync(List<Variable> variables);

    /// <summary>
    /// 查找已存在的变量。
    /// </summary>
    Task<List<Variable>> FindExistingVariablesAsync(IEnumerable<Variable> variablesToCheck);

    /// <summary>
    /// 异步加载所有变量数据到内存中。
    /// </summary>
    Task LoadAllVariablesAsync();
}