using AutoMapper;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.Application.Services.Database;

/// <summary>
/// IMqttAliasAppService 的实现，负责管理变量与MQTT服务器的别名关联。
/// </summary>
public class MqttAliasAppService : IMqttAliasAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数。
    /// </summary>
    public MqttAliasAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步获取指定变量的所有MQTT别名关联。
    /// </summary>
    public async Task<List<MqttAlias>> GetAliasesForVariableAsync(int variableId)
    {
        // 从仓储获取别名，并确保加载了关联的MqttServer信息
        var aliases = await _repoManager.VariableMqttAliases.GetAliasesForVariableAsync(variableId);
        return aliases.ToList();
    }

    /// <summary>
    /// 异步为变量分配或更新一个MQTT别名。
    /// </summary>
    public async Task AssignAliasAsync(int variableId, int mqttServerId, string alias)
    {
        try
        {
            await _repoManager.BeginTranAsync();

            // 检查是否已存在该变量与该服务器的关联
            var existingAlias = await _repoManager.VariableMqttAliases.GetByVariableAndServerAsync(variableId, mqttServerId);

            if (existingAlias != null)
            {
                // 如果存在，则更新别名
                existingAlias.Alias = alias;
                await _repoManager.VariableMqttAliases.UpdateAsync(existingAlias);
            }
            else
            {
                // 如果不存在，则创建新的关联
                // 获取关联的Variable和MqttServer实体
                var variable = await _repoManager.Variables.GetByIdAsync(variableId);
                var mqttServer = await _repoManager.MqttServers.GetByIdAsync(mqttServerId);
                
                var newAlias = new MqttAlias
                {
                    VariableId = variableId,
                    MqttServerId = mqttServerId,
                    Alias = alias,
                    Variable = variable,
                    MqttServer = mqttServer
                };
                await _repoManager.VariableMqttAliases.AddAsync(newAlias);
            }

            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("分配/更新MQTT别名失败。", ex);
        }
    }

    /// <summary>
    /// 异步更新一个已存在的MQTT别名。
    /// </summary>
    public async Task UpdateAliasAsync(int aliasId, string newAlias)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var aliasToUpdate = await _repoManager.VariableMqttAliases.GetByIdAsync(aliasId);
            if (aliasToUpdate == null)
            {
                throw new KeyNotFoundException($"未找到ID为 {aliasId} 的MQTT别名关联。");
            }
            aliasToUpdate.Alias = newAlias;
            await _repoManager.VariableMqttAliases.UpdateAsync(aliasToUpdate);
            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("更新MQTT别名失败。", ex);
        }
    }

    /// <summary>
    /// 异步移除一个MQTT别名关联。
    /// </summary>
    public async Task RemoveAliasAsync(int aliasId)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            await _repoManager.VariableMqttAliases.DeleteByIdAsync(aliasId);
            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("移除MQTT别名失败。", ex);
        }
    }
}