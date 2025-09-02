using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace DMS.Application.Services;

/// <summary>
/// 变量应用服务，负责处理变量相关的业务逻辑。
/// 实现 <see cref="IVariableAppService"/> 接口。
/// </summary>
public class VariableAppService : IVariableAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，通过依赖注入获取仓储管理器和AutoMapper实例。
    /// </summary>
    /// <param name="repoManager">仓储管理器实例。</param>
    /// <param name="mapper">AutoMapper 实例。</param>
    public VariableAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取变量数据传输对象。
    /// </summary>
    /// <param name="id">变量ID。</param>
    /// <returns>变量数据传输对象。</returns>
    public async Task<VariableDto> GetVariableByIdAsync(int id)
    {
        var variable = await _repoManager.Variables.GetByIdAsync(id);
        return _mapper.Map<VariableDto>(variable);
    }

    /// <summary>
    /// 异步获取所有变量数据传输对象列表。
    /// </summary>
    /// <returns>变量数据传输对象列表。</returns>
    public async Task<List<VariableDto>> GetAllVariablesAsync()
    {
        var variables = await _repoManager.Variables.GetAllAsync();
        return _mapper.Map<List<VariableDto>>(variables);
    }

    /// <summary>
    /// 异步创建一个新变量（事务性操作）。
    /// </summary>
    /// <param name="variableDto">要创建的变量数据传输对象。</param>
    /// <returns>新创建的变量数据传输对象。</returns>
    /// <exception cref="ApplicationException">如果创建变量时发生错误。</exception>
    public async Task<VariableDto> CreateVariableAsync(VariableDto variableDto)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var variable = _mapper.Map<Variable>(variableDto);
            var addedVariable = await _repoManager.Variables.AddAsync(variable);
            await _repoManager.CommitAsync();
            return _mapper.Map<VariableDto>(addedVariable);
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException($"创建变量时发生错误，操作已回滚,错误信息:{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步更新一个已存在的变量（事务性操作）。
    /// </summary>
    /// <param name="variableDto">要更新的变量数据传输对象。</param>
    /// <returns>受影响的行数。</returns>
    /// <exception cref="ApplicationException">如果找不到变量或更新变量时发生错误。</exception>
    public async Task<int> UpdateVariableAsync(VariableDto variableDto)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var variable = await _repoManager.Variables.GetByIdAsync(variableDto.Id);
            if (variable == null)
            {
                throw new ApplicationException($"Variable with ID {variableDto.Id} not found.");
            }
            _mapper.Map(variableDto, variable);
            int res = await _repoManager.Variables.UpdateAsync(variable);
            await _repoManager.CommitAsync();
            return res;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException($"更新变量时发生错误，操作已回滚,错误信息:{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步批量更新变量（事务性操作）。
    /// </summary>
    /// <param name="variableDtos">要更新的变量数据传输对象列表。</param>
    /// <returns>受影响的行数。</returns>
    /// <exception cref="ApplicationException">如果更新变量时发生错误。</exception>
    public async Task<int> UpdateVariablesAsync(List<VariableDto> variableDtos)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            int totalAffected = 0;
            
            foreach (var variableDto in variableDtos)
            {
                var variable = await _repoManager.Variables.GetByIdAsync(variableDto.Id);
                if (variable == null)
                {
                    throw new ApplicationException($"Variable with ID {variableDto.Id} not found.");
                }
                _mapper.Map(variableDto, variable);
                int res = await _repoManager.Variables.UpdateAsync(variable);
                totalAffected += res;
            }
            
            await _repoManager.CommitAsync();
            return totalAffected;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException($"批量更新变量时发生错误，操作已回滚,错误信息:{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步删除一个变量（事务性操作）。
    /// </summary>
    /// <param name="id">要删除变量的ID。</param>
    /// <returns>如果删除成功则为 true，否则为 false。</returns>
    /// <exception cref="InvalidOperationException">如果删除变量失败。</exception>
    /// <exception cref="ApplicationException">如果删除变量时发生其他错误。</exception>
    public async Task<bool> DeleteVariableAsync(int id)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var delRes = await _repoManager.Variables.DeleteByIdAsync(id);
            if (delRes == 0)
            {
                throw new InvalidOperationException($"删除变量失败：变量ID:{id}，请检查变量Id是否存在");
            }
            await _repoManager.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException($"删除变量时发生错误，操作已回滚,错误信息:{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步批量删除变量（事务性操作）。
    /// </summary>
    /// <param name="ids">要删除的变量ID列表。</param>
    /// <returns>如果删除成功则为 true，否则为 false。</returns>
    /// <exception cref="ArgumentException">如果ID列表为空或null。</exception>
    /// <exception cref="ApplicationException">如果删除变量时发生错误。</exception>
    public async Task<bool> DeleteVariablesAsync(List<int> ids)
    {
        if (ids == null || !ids.Any())
        {
            throw new ArgumentException("变量ID列表不能为空", nameof(ids));
        }

        try
        {
            await _repoManager.BeginTranAsync();
            
            // 批量删除变量
            var deletedCount = await _repoManager.Variables.DeleteByIdsAsync(ids);
            
            // 检查是否所有变量都被成功删除
            if (deletedCount != ids.Count)
            {
                throw new InvalidOperationException($"删除变量失败：请求删除 {ids.Count} 个变量，实际删除 {deletedCount} 个变量");
            }
            
            await _repoManager.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException($"批量删除变量时发生错误，操作已回滚,错误信息:{ex.Message}", ex);
        }
    }

    public async Task<bool> BatchImportVariablesAsync(List<VariableDto> variables)
    {
        try
        {
            var variableModels = _mapper.Map<List<Variable>>(variables);
            var result = await _repoManager.Variables.AddBatchAsync(variableModels);
            return result;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"批量导入变量时发生错误,错误信息:{ex.Message}", ex);
        }
    }

    public async Task<List<VariableDto>> FindExistingVariablesAsync(IEnumerable<VariableDto> variablesToCheck)
    {
        if (variablesToCheck == null || !variablesToCheck.Any())
        {
            return new List<VariableDto>();
        }

        var names = variablesToCheck.Select(v => v.Name).Where(n => !string.IsNullOrEmpty(n)).Distinct().ToList();
        var s7Addresses = variablesToCheck.Select(v => v.S7Address).Where(a => !string.IsNullOrEmpty(a)).Distinct().ToList();
        var opcUaNodeIds = variablesToCheck.Select(v => v.OpcUaNodeId).Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList();

        var allVariables = await _repoManager.Variables.GetAllAsync();
        var existingVariablesFromDb = allVariables.Where(v =>
            (names.Any() && !string.IsNullOrEmpty(v.Name) && names.Contains(v.Name)) ||
            (s7Addresses.Any() && !string.IsNullOrEmpty(v.S7Address) && s7Addresses.Contains(v.S7Address)) ||
            (opcUaNodeIds.Any() && !string.IsNullOrEmpty(v.OpcUaNodeId) && opcUaNodeIds.Contains(v.OpcUaNodeId)))
            .ToList();

        if (existingVariablesFromDb == null || !existingVariablesFromDb.Any())
        {
            return new List<VariableDto>();
        }

        var existingNames = new HashSet<string>(existingVariablesFromDb.Select(v => v.Name).Where(n => !string.IsNullOrEmpty(n)));
        var existingS7Addresses = new HashSet<string>(existingVariablesFromDb.Select(v => v.S7Address).Where(a => !string.IsNullOrEmpty(a)));
        var existingOpcUaNodeIds = new HashSet<string>(existingVariablesFromDb.Select(v => v.OpcUaNodeId).Where(id => !string.IsNullOrEmpty(id)));

        var result = variablesToCheck.Where(v =>
            (!string.IsNullOrEmpty(v.Name) && existingNames.Contains(v.Name)) ||
            (!string.IsNullOrEmpty(v.S7Address) && existingS7Addresses.Contains(v.S7Address)) ||
            (!string.IsNullOrEmpty(v.OpcUaNodeId) && existingOpcUaNodeIds.Contains(v.OpcUaNodeId)))
            .ToList();

        return result;
    }

    public async Task<VariableDto?> FindExistingVariableAsync(VariableDto variableToCheck)
    {
        if (variableToCheck == null)
        {
            return null;
        }

        // 创建一个包含单个元素的列表以便复用现有的逻辑
        var variablesToCheck = new List<VariableDto> { variableToCheck };
        var existingVariables = await FindExistingVariablesAsync(variablesToCheck);

        // 如果找到了匹配的变量，返回第一个（也是唯一一个）
        return existingVariables.FirstOrDefault();
    }
}