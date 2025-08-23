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
    /// 异步获取所有变量DTO列表。
    /// </summary>
    Task<List<VariableDto>> GetAllVariablesAsync();

    /// <summary>
    /// 异步创建一个新变量。
    /// </summary>
    Task<int> CreateVariableAsync(VariableDto variableDto);

    /// <summary>
    /// 异步更新一个已存在的变量。
    /// </summary>
    Task<int> UpdateVariableAsync(VariableDto variableDto);

    /// <summary>
    /// 异步删除一个变量。
    /// </summary>
    Task<bool> DeleteVariableAsync(int id);

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
}