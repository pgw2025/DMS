using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;

namespace DMS.Application.Services;

/// <summary>
/// 实现MQTT服务器管理的应用服务。
/// </summary>
public class MqttAppService : IMqttAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    public MqttAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    public async Task<MqttServerDto> GetMqttServerByIdAsync(int id)
    {
        var mqttServer = await _repoManager.MqttServers.GetByIdAsync(id);
        return _mapper.Map<MqttServerDto>(mqttServer);
    }

    public async Task<List<MqttServerDto>> GetAllMqttServersAsync()
    {
        var mqttServers = await _repoManager.MqttServers.GetAllAsync();
        return _mapper.Map<List<MqttServerDto>>(mqttServers);
    }

    public async Task<int> CreateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        var mqttServer = _mapper.Map<MqttServer>(mqttServerDto);
        await _repoManager.MqttServers.AddAsync(mqttServer);
        await _repoManager.CommitAsync();
        return mqttServer.Id;
    }

    public async Task UpdateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        var mqttServer = await _repoManager.MqttServers.GetByIdAsync(mqttServerDto.Id);
        if (mqttServer == null)
        {
            throw new ApplicationException($"MQTT Server with ID {mqttServerDto.Id} not found.");
        }
        _mapper.Map(mqttServerDto, mqttServer);
        await _repoManager.MqttServers.UpdateAsync(mqttServer);
        await _repoManager.CommitAsync();
    }

    public async Task DeleteMqttServerAsync(int id)
    {
        await _repoManager.MqttServers.DeleteAsync(id);
        await _repoManager.CommitAsync();
    }
}