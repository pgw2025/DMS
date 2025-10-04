using DMS.Application.DTOs;
using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;

namespace DMS.Application.Services.Management;

/// <summary>
/// MQTT管理服务，负责MQTT相关的业务逻辑。
/// </summary>
public class MqttManagementService : IMqttManagementService
{
    private readonly IMqttAppService _mqttAppService;
    private readonly IAppDataStorageService _appDataStorageService;
    private readonly IEventService _eventService;

    public MqttManagementService(IMqttAppService mqttAppService, IAppDataStorageService appDataStorageService, IEventService eventService)
    {
        _mqttAppService = mqttAppService;
        _appDataStorageService = appDataStorageService;
        _eventService = eventService;
    }

    /// <summary>
    /// 异步根据ID获取MQTT服务器DTO。
    /// </summary>
    public async Task<MqttServerDto> GetMqttServerByIdAsync(int id)
    {
        return await _mqttAppService.GetMqttServerByIdAsync(id);
    }

    /// <summary>
    /// 异步获取所有MQTT服务器DTO列表。
    /// </summary>
    public async Task<List<MqttServerDto>> GetAllMqttServersAsync()
    {
        return await _mqttAppService.GetAllMqttServersAsync();
    }

    /// <summary>
    /// 异步创建一个新的MQTT服务器。
    /// </summary>
    public async Task<int> CreateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        var result = await _mqttAppService.CreateMqttServerAsync(mqttServerDto);
        
        // 创建成功后，将MQTT服务器添加到内存中
        if (result > 0)
        {
            mqttServerDto.Id = result; // 假设返回的ID是新创建的
            if (_appDataStorageService.MqttServers.TryAdd(mqttServerDto.Id, mqttServerDto))
            {
                _eventService.RaiseMqttServerChanged(this, new MqttServerChangedEventArgs(ActionChangeType.Added, mqttServerDto));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步更新一个已存在的MQTT服务器。
    /// </summary>
    public async Task UpdateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        await _mqttAppService.UpdateMqttServerAsync(mqttServerDto);
        
        // 更新成功后，更新内存中的MQTT服务器
        _appDataStorageService.MqttServers.AddOrUpdate(mqttServerDto.Id, mqttServerDto, (key, oldValue) => mqttServerDto);
        _eventService.RaiseMqttServerChanged(this, new MqttServerChangedEventArgs(ActionChangeType.Updated, mqttServerDto));
    }

    /// <summary>
    /// 异步删除一个MQTT服务器。
    /// </summary>
    public async Task DeleteMqttServerAsync(int id)
    {
        var mqttServer = await _mqttAppService.GetMqttServerByIdAsync(id); // 获取MQTT服务器信息用于内存删除
        var result = await _mqttAppService.DeleteMqttServerAsync(id);
        
        // 删除成功后，从内存中移除MQTT服务器
        if (result>0)
        {
            if (_appDataStorageService.MqttServers.TryRemove(id, out var mqttServerDto))
            {
                _eventService.RaiseMqttServerChanged(this, new MqttServerChangedEventArgs(ActionChangeType.Deleted, mqttServerDto));
            }
        }
    }


}