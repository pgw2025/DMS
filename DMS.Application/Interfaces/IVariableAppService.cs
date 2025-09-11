using DMS.Application.DTOs;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义变量管理相关的应用服务操作。
/// </summary>
public interface IVariableAppService
{
    /// <summary>
    /// 异步根据ID获取变量DTO。
    /// </summary>
    Task<VariableDto> GetVariableByIdAsync(int id);

    /// <summary>
    /// 异步根据OPC UA NodeId获取变量DTO。
    /// </summary>
    Task<VariableDto?> GetVariableByOpcUaNodeIdAsync(string opcUaNodeId);

    /// <summary>
    /// 异步根据OPC UA NodeId列表获取变量DTO列表。
    /// </summary>
    Task<List<VariableDto>> GetVariableByOpcUaNodeIdsAsync(List<string> opcUaNodeIds);

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
    /// 异步更新一个已存在的变量。
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
    /// 异步批量导入变量。
    /// </summary>
    Task<bool> BatchImportVariablesAsync(List<VariableDto> variables);

    /// <summary>
    /// 检测一组变量是否已存在。
    /// </summary>
    /// <param name="variablesToCheck">要检查的变量列表。</param>
    /// <returns>返回输入列表中已存在的变量。</returns>
    Task<List<VariableDto>> FindExistingVariablesAsync(IEnumerable<VariableDto> variablesToCheck);

    /// <summary>
    /// 检测单个变量是否已存在。
    /// </summary>
    /// <param name="variableToCheck">要检查的变量。</param>
    /// <returns>如果变量已存在则返回该变量，否则返回null。</returns>
    Task<VariableDto?> FindExistingVariableAsync(VariableDto variableToCheck);
    
    /// <summary>
    /// 异步获取指定变量的历史记录。
    /// </summary>
    /// <param name="variableId">变量ID</param>
    /// <returns>变量历史记录列表</returns>
    Task<List<VariableHistoryDto>> GetVariableHistoriesAsync(int variableId);
}