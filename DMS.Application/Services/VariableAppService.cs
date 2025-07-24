using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;

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
    /// <returns>新创建变量的ID。</returns>
    /// <exception cref="ApplicationException">如果创建变量时发生错误。</exception>
    public async Task<int> CreateVariableAsync(VariableDto variableDto)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var variable = _mapper.Map<Variable>(variableDto);
            var addedVariable = await _repoManager.Variables.AddAsync(variable);
            await _repoManager.CommitAsync();
            return addedVariable.Id;
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
}