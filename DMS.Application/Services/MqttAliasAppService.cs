using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;

namespace DMS.Application.Services;

/// <summary>
/// 实现MQTT别名管理的应用服务。
/// </summary>
public class MqttAliasAppService : IMqttAliasAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    public MqttAliasAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    public async Task<VariableMqttAliasDto> GetMqttAliasByIdAsync(int id)
    {
        var mqttAlias = await _repoManager.VariableMqttAliases.GetByIdAsync(id);
        return _mapper.Map<VariableMqttAliasDto>(mqttAlias);
    }

    public async Task<List<VariableMqttAliasDto>> GetAllMqttAliasesAsync()
    {
        var mqttAliases = await _repoManager.VariableMqttAliases.GetAllAsync();
        return _mapper.Map<List<VariableMqttAliasDto>>(mqttAliases);
    }

    public async Task<int> CreateMqttAliasAsync(VariableMqttAliasDto mqttAliasDto)
    {
        var mqttAlias = _mapper.Map<VariableMqttAlias>(mqttAliasDto);
        await _repoManager.VariableMqttAliases.AddAsync(mqttAlias);
        await _repoManager.CommitAsync();
        return mqttAlias.Id;
    }

    public async Task UpdateMqttAliasAsync(VariableMqttAliasDto mqttAliasDto)
    {
        var mqttAlias = await _repoManager.VariableMqttAliases.GetByIdAsync(mqttAliasDto.Id);
        if (mqttAlias == null)
        {
            throw new ApplicationException($"MQTT Alias with ID {mqttAliasDto.Id} not found.");
        }
        _mapper.Map(mqttAliasDto, mqttAlias);
        await _repoManager.VariableMqttAliases.UpdateAsync(mqttAlias);
        await _repoManager.CommitAsync();
    }

    public async Task DeleteMqttAliasAsync(int id)
    {
        await _repoManager.VariableMqttAliases.DeleteAsync(id);
        await _repoManager.CommitAsync();
    }
}