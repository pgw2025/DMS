using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;

namespace DMS.Application.Services;

/// <summary>
/// 实现变量管理的应用服务。
/// </summary>
public class VariableAppService : IVariableAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    public VariableAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    public async Task<VariableDto> GetVariableByIdAsync(int id)
    {
        var variable = await _repoManager.Variables.GetByIdAsync(id);
        return _mapper.Map<VariableDto>(variable);
    }

    public async Task<List<VariableDto>> GetAllVariablesAsync()
    {
        var variables = await _repoManager.Variables.GetAllAsync();
        return _mapper.Map<List<VariableDto>>(variables);
    }

    public async Task<int> CreateVariableAsync(VariableDto variableDto)
    {
        var variable = _mapper.Map<Variable>(variableDto);
        await _repoManager.Variables.AddAsync(variable);
        await _repoManager.CommitAsync();
        return variable.Id;
    }

    public async Task UpdateVariableAsync(VariableDto variableDto)
    {
        var variable = await _repoManager.Variables.GetByIdAsync(variableDto.Id);
        if (variable == null)
        {
            throw new ApplicationException($"Variable with ID {variableDto.Id} not found.");
        }
        _mapper.Map(variableDto, variable);
        await _repoManager.Variables.UpdateAsync(variable);
        await _repoManager.CommitAsync();
    }

    public async Task DeleteVariableAsync(int id)
    {
        await _repoManager.Variables.DeleteAsync(id);
        await _repoManager.CommitAsync();
    }
}