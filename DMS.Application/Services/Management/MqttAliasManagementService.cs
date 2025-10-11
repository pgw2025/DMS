using AutoMapper;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Application.Services.Management;

public class MqttAliasManagementService : IMqttAliasManagementService
{
    private readonly IMqttAliasAppService _appService;
    private readonly IEventService _eventService;
    private readonly IAppDataStorageService _storageService;
    private readonly IMapper _mapper;

    public MqttAliasManagementService(IMqttAliasAppService appService, IEventService eventService,
                                      IAppDataStorageService storageService, IMapper mapper)
    {
        _appService = appService;
        _eventService = eventService;
        _storageService = storageService;
        _mapper = mapper;
    }

    public async Task<MqttAlias> AssignAliasAsync(MqttAlias alias)
    {
        var newAlias = await _appService.AssignAliasAsync(alias);
        if (newAlias != null)
        {
            // Add to cache
            if (_storageService.MqttServers.TryGetValue(newAlias.MqttServerId, out var server))
            {
                newAlias.MqttServer = server;
                server.VariableAliases.Add(newAlias);
            }

            // Add to cache
            if (_storageService.Variables.TryGetValue(newAlias.VariableId, out var variable))
            {
                newAlias.Variable = variable;
                variable.MqttAliases.Add(newAlias);
            }
            _storageService.MqttAliases.TryAdd(newAlias.Id, newAlias);
            _eventService.RaiseMqttAliasChanged(this, new MqttAliasChangedEventArgs(ActionChangeType.Added, newAlias));
        }

        return newAlias;
    }

    public async Task<List<MqttAlias>> LoadAllMqttAliasAsync()
    {
        var mqttAliases = await _appService.GetAllAsync();
        foreach (var mqttAlias in mqttAliases)
        {
            // Add to cache
            if (_storageService.MqttServers.TryGetValue(mqttAlias.MqttServerId, out var server))
            {
                mqttAlias.MqttServer = server;
                server.VariableAliases.Add(mqttAlias);
            }

            // Add to cache
            if (_storageService.Variables.TryGetValue(mqttAlias.VariableId, out var variable))
            {
                mqttAlias.Variable= variable;
                variable.MqttAliases.Add(mqttAlias);
            }
            _storageService.MqttAliases.TryAdd(mqttAlias.Id, mqttAlias);
            _eventService.RaiseMqttAliasChanged(this, new MqttAliasChangedEventArgs(ActionChangeType.Added, mqttAlias));
        }

        return mqttAliases;
    }

    public async Task<int> UpdateAsync(MqttAlias alias)
    {
        int res = await _appService.UpdateAliasAsync(alias);
        if (res>0)
        {
            // Add to cache
            if (_storageService.MqttAliases.TryGetValue(alias.Id, out var mqttAlias))
            {
                mqttAlias.Alias = alias.Alias;
            }
        }
        return res;
    }



    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _appService.RemoveAliasAsync(id);
        if (result == 0) return false;

        if (_storageService.MqttAliases.TryGetValue(id, out var mqttAlias))
        {

            mqttAlias.MqttServer.VariableAliases.Remove(mqttAlias);
            mqttAlias.Variable.MqttAliases.Remove(mqttAlias);
            _storageService.MqttAliases.TryRemove(mqttAlias.Id,out _);
            _eventService.RaiseMqttAliasChanged(
                this, new MqttAliasChangedEventArgs(ActionChangeType.Deleted, mqttAlias));

        }
        return true;
    }
}