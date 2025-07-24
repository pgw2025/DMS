using AutoMapper;
using DMS.Core.Interfaces;
using DMS.Core.Models;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;

namespace DMS.Application.Services;

/// <summary>
/// MQTT别名应用服务，负责处理MQTT别名相关的业务逻辑。
/// 实现 <see cref="IMqttAliasAppService"/> 接口。
/// </summary>
public class MqttAliasAppService : IMqttAliasAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，通过依赖注入获取仓储管理器和AutoMapper实例。
    /// </summary>
    /// <param name="repoManager">仓储管理器实例。</param>
    /// <param name="mapper">AutoMapper 实例。</param>
    public MqttAliasAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取MQTT别名数据传输对象。
    /// </summary>
    /// <param name="id">MQTT别名ID。</param>
    /// <returns>MQTT别名数据传输对象。</returns>
    public async Task<VariableMqttAliasDto> GetMqttAliasByIdAsync(int id)
    {
        var mqttAlias = await _repoManager.VariableMqttAliases.GetByIdAsync(id);
        return _mapper.Map<VariableMqttAliasDto>(mqttAlias);
    }

    /// <summary>
    /// 异步获取所有MQTT别名数据传输对象列表。
    /// </summary>
    /// <returns>MQTT别名数据传输对象列表。</returns>
    public async Task<List<VariableMqttAliasDto>> GetAllMqttAliasesAsync()
    {
        var mqttAliases = await _repoManager.VariableMqttAliases.GetAllAsync();
        return _mapper.Map<List<VariableMqttAliasDto>>(mqttAliases);
    }

    /// <summary>
    /// 异步创建一个新MQTT别名（事务性操作）。
    /// </summary>
    /// <param name="mqttAliasDto">要创建的MQTT别名数据传输对象。</param>
    /// <returns>新创建MQTT别名的ID。</returns>
    /// <exception cref="ApplicationException">如果创建MQTT别名时发生错误。</exception>
    public async Task<int> CreateMqttAliasAsync(VariableMqttAliasDto mqttAliasDto)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var mqttAlias = _mapper.Map<VariableMqttAlias>(mqttAliasDto);
            await _repoManager.VariableMqttAliases.AddAsync(mqttAlias);
            await _repoManager.CommitAsync();
            return mqttAlias.Id;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("创建MQTT别名时发生错误，操作已回滚。", ex);
        }
    }

    /// <summary>
    /// 异步更新一个已存在的MQTT别名（事务性操作）。
    /// </summary>
    /// <param name="mqttAliasDto">要更新的MQTT别名数据传输对象。</param>
    /// <returns>表示异步操作的任务。</returns>
    /// <exception cref="ApplicationException">如果找不到MQTT别名或更新MQTT别名时发生错误。</exception>
    public async Task UpdateMqttAliasAsync(VariableMqttAliasDto mqttAliasDto)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var mqttAlias = await _repoManager.VariableMqttAliases.GetByIdAsync(mqttAliasDto.Id);
            if (mqttAlias == null)
            {
                throw new ApplicationException($"MQTT Alias with ID {mqttAliasDto.Id} not found.");
            }
            _mapper.Map(mqttAliasDto, mqttAlias);
            await _repoManager.VariableMqttAliases.UpdateAsync(mqttAlias);
            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("更新MQTT别名时发生错误，操作已回滚。", ex);
        }
    }

    /// <summary>
    /// 异步删除一个MQTT别名（事务性操作）。
    /// </summary>
    /// <param name="id">要删除MQTT别名的ID。</param>
    /// <returns>表示异步操作的任务。</returns>
    /// <exception cref="ApplicationException">如果删除MQTT别名时发生错误。</exception>
    public async Task DeleteMqttAliasAsync(int id)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            await _repoManager.VariableMqttAliases.DeleteByIdAsync(id);
            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("删除MQTT别名时发生错误，操作已回滚。", ex);
        }
    }
}