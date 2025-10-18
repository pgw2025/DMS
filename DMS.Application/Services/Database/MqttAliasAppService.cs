using AutoMapper;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Core.Interfaces;
using DMS.Core.Models;

namespace DMS.Application.Services.Database;

/// <summary>
/// IMqttAliasAppService 的实现，负责管理变量与MQTT服务器的别名关联。
/// </summary>
public class MqttAliasAppService : IMqttAliasAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IAppStorageService _appStorageService;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数。
    /// </summary>
    public MqttAliasAppService(IRepositoryManager repoManager,IAppStorageService appStorageService, IMapper mapper)
    {
        _repoManager = repoManager;
        _appStorageService = appStorageService;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步为变量分配或更新一个MQTT别名。
    /// </summary>
    public async Task<MqttAlias> AssignAliasAsync(MqttAlias mqttAlias)
    {
        return await _repoManager.MqttAliases.AddAsync(mqttAlias);
    }

    /// <summary>
    /// 异步更新一个已存在的MQTT别名。
    /// </summary>
    public async Task<int> UpdateAliasAsync(MqttAlias mqttAlias)
    {
        return await _repoManager.MqttAliases.UpdateAsync(mqttAlias);
    }

    /// <summary>
    /// 异步移除一个MQTT别名关联。
    /// </summary>
    public async Task<int> RemoveAliasAsync(int aliasId)
    {
        return await _repoManager.MqttAliases.DeleteByIdAsync(aliasId);
    }

    public async Task<List<MqttAlias>> GetAllAsync()
    {
        var mqttAliases = await _repoManager.MqttAliases.GetAllAsync();
       
        return mqttAliases;
    }
}