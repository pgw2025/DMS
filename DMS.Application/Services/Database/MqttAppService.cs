using AutoMapper;
using DMS.Application.DTOs;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Core.Interfaces;
using DMS.Core.Models;

namespace DMS.Application.Services.Database;

/// <summary>
/// MQTT应用服务，负责处理MQTT服务器相关的业务逻辑。
/// 实现 <see cref="IMqttAppService"/> 接口。
/// </summary>
public class MqttAppService : IMqttAppService
{
    private readonly IRepositoryManager _repoManager;
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，通过依赖注入获取仓储管理器和AutoMapper实例。
    /// </summary>
    /// <param name="repoManager">仓储管理器实例。</param>
    /// <param name="mapper">AutoMapper 实例。</param>
    public MqttAppService(IRepositoryManager repoManager, IMapper mapper)
    {
        _repoManager = repoManager;
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取MQTT服务器数据传输对象。
    /// </summary>
    /// <param name="id">MQTT服务器ID。</param>
    /// <returns>MQTT服务器数据传输对象。</returns>
    public async Task<MqttServerDto> GetMqttServerByIdAsync(int id)
    {
        var mqttServer = await _repoManager.MqttServers.GetByIdAsync(id);
        return _mapper.Map<MqttServerDto>(mqttServer);
    }

    /// <summary>
    /// 异步获取所有MQTT服务器数据传输对象列表。
    /// </summary>
    /// <returns>MQTT服务器数据传输对象列表。</returns>
    public async Task<List<MqttServerDto>> GetAllMqttServersAsync()
    {
        var mqttServers = await _repoManager.MqttServers.GetAllAsync();
        return _mapper.Map<List<MqttServerDto>>(mqttServers);
    }

    /// <summary>
    /// 异步创建一个新MQTT服务器（事务性操作）。
    /// </summary>
    /// <param name="mqttServerDto">要创建的MQTT服务器数据传输对象。</param>
    /// <returns>新创建MQTT服务器的ID。</returns>
    /// <exception cref="ApplicationException">如果创建MQTT服务器时发生错误。</exception>
    public async Task<int> CreateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var mqttServer = _mapper.Map<MqttServer>(mqttServerDto);
            await _repoManager.MqttServers.AddAsync(mqttServer);
            await _repoManager.CommitAsync();
            return mqttServer.Id;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("创建MQTT服务器时发生错误，操作已回滚。", ex);
        }
    }

    /// <summary>
    /// 异步更新一个已存在的MQTT服务器（事务性操作）。
    /// </summary>
    /// <param name="mqttServerDto">要更新的MQTT服务器数据传输对象。</param>
    /// <returns>表示异步操作的任务。</returns>
    /// <exception cref="ApplicationException">如果找不到MQTT服务器或更新MQTT服务器时发生错误。</exception>
    public async Task UpdateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var mqttServer = await _repoManager.MqttServers.GetByIdAsync(mqttServerDto.Id);
            if (mqttServer == null)
            {
                throw new ApplicationException($"MQTT Server with ID {mqttServerDto.Id} not found.");
            }
            _mapper.Map(mqttServerDto, mqttServer);
            await _repoManager.MqttServers.UpdateAsync(mqttServer);
            await _repoManager.CommitAsync();
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("更新MQTT服务器时发生错误，操作已回滚。", ex);
        }
    }


    /// <summary>
    /// 异步根据ID删除一个MQTT服务器（事务性操作）。
    /// </summary>
    /// <param name="id">要删除MQTT服务器的ID。</param>
    /// <returns>如果删除成功则为 true，否则为 false。</returns>
    /// <exception cref="ApplicationException">如果删除MQTT服务器时发生错误。</exception>
    public async Task<int> DeleteMqttServerAsync(int id)
    {
        try
        {
            return await _repoManager.MqttServers.DeleteByIdAsync(id);
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException($"删除MQTT服务器时发生错误，操作已回滚,错误信息:{ex.Message}", ex);
        }
    }

    /// <summary>
    /// 异步批量更新MQTT服务器（事务性操作）。
    /// </summary>
    /// <param name="mqttServerDtos">要更新的MQTT服务器数据传输对象列表。</param>
    /// <returns>成功更新的MQTT服务器数量。</returns>
    /// <exception cref="ApplicationException">如果批量更新MQTT服务器时发生错误。</exception>
    public async Task<int> UpdateMqttServersAsync(List<MqttServerDto> mqttServerDtos)
    {
        try
        {
            await _repoManager.BeginTranAsync();
            var count = 0;
            foreach (var mqttServerDto in mqttServerDtos)
            {
                var mqttServer = await _repoManager.MqttServers.GetByIdAsync(mqttServerDto.Id);
                if (mqttServer != null)
                {
                    _mapper.Map(mqttServerDto, mqttServer);
                    await _repoManager.MqttServers.UpdateAsync(mqttServer);
                    count++;
                }
            }
            await _repoManager.CommitAsync();
            return count;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException("批量更新MQTT服务器时发生错误，操作已回滚。", ex);
        }
    }

    /// <summary>
    /// 异步批量删除MQTT服务器（事务性操作）。
    /// </summary>
    /// <param name="ids">要删除的MQTT服务器ID列表。</param>
    /// <returns>如果删除成功则为 true，否则为 false。</returns>
    /// <exception cref="ApplicationException">如果批量删除MQTT服务器时发生错误。</exception>
    public async Task<bool> DeleteMqttServersAsync(List<int> ids)
    {
        try
        {
            if (ids == null || !ids.Any()) return true;

            await _repoManager.BeginTranAsync();
            var result = await _repoManager.MqttServers.DeleteByIdsAsync(ids);
            await _repoManager.CommitAsync();
            return result > 0;
        }
        catch (Exception ex)
        {
            await _repoManager.RollbackAsync();
            throw new ApplicationException($"批量删除MQTT服务器时发生错误，操作已回滚,错误信息:{ex.Message}", ex);
        }
    }
}