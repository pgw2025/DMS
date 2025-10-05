using System.Collections.Concurrent;
using AutoMapper;
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
    private readonly IMapper _mapper;
    private readonly IDataProcessingService _dataProcessingService;

    public MqttManagementService(IMqttAppService mqttAppService, 
                                IAppDataStorageService appDataStorageService, 
                                IEventService eventService,
                                IMapper mapper,
                                IDataProcessingService dataProcessingService)
    {
        _mqttAppService = mqttAppService;
        _appDataStorageService = appDataStorageService;
        _eventService = eventService;
        _mapper = mapper;
        _dataProcessingService = dataProcessingService;
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
    public async Task<MqttServerDto> CreateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        var result = await _mqttAppService.CreateMqttServerAsync(mqttServerDto);
        
        // 创建成功后，将MQTT服务器添加到内存中
        if (result > 0)
        {
            mqttServerDto.Id = result; // 假设返回的ID是新创建的
            if (_appDataStorageService.MqttServers.TryAdd(mqttServerDto.Id, mqttServerDto))
            {
                _eventService.RaiseMqttServerChanged(
                    this, new MqttServerChangedEventArgs(ActionChangeType.Added, mqttServerDto));
            }
        }
        
        return mqttServerDto;
    }

    /// <summary>
    /// 异步更新一个已存在的MQTT服务器。
    /// </summary>
    public async Task<int> UpdateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        return await UpdateMqttServersAsync(new List<MqttServerDto>() { mqttServerDto });
    }

    /// <summary>
    /// 异步批量更新MQTT服务器。
    /// </summary>
    public async Task<int> UpdateMqttServersAsync(List<MqttServerDto> mqttServerDtos)
    {
        var result = await _mqttAppService.UpdateMqttServersAsync(mqttServerDtos);
        
        // 批量更新成功后，更新内存中的MQTT服务器
        if (result > 0 && mqttServerDtos != null)
        {
            foreach (var mqttServerDto in mqttServerDtos)
            {
                if (_appDataStorageService.MqttServers.TryGetValue(mqttServerDto.Id, out var mMqttServerDto))
                {
                    // 比较旧值和新值，确定哪个属性发生了变化
                    var changedProperties = GetChangedProperties(mMqttServerDto, mqttServerDto);
                    
                    // 更新内存中的MQTT服务器
                    _mapper.Map(mqttServerDto, mMqttServerDto);

                    // 为每个发生变化的属性触发事件
                    foreach (var property in changedProperties)
                    {
                        _eventService.RaiseMqttServerChanged(
                            this, new MqttServerChangedEventArgs(ActionChangeType.Updated, mMqttServerDto, property));
                    }
                    
                    // 如果没有任何属性发生变化，至少触发一次更新事件
                    if (changedProperties.Count == 0)
                    {
                        _eventService.RaiseMqttServerChanged(
                            this, new MqttServerChangedEventArgs(ActionChangeType.Updated, mMqttServerDto, MqttServerPropertyType.All));
                    }
                }
                else
                {
                    // 如果内存中不存在该MQTT服务器，则直接添加
                    _appDataStorageService.MqttServers.TryAdd(mqttServerDto.Id, mqttServerDto);
                    _eventService.RaiseMqttServerChanged(
                        this, new MqttServerChangedEventArgs(ActionChangeType.Added, mqttServerDto, MqttServerPropertyType.All));
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步删除一个MQTT服务器。
    /// </summary>
    public async Task<bool> DeleteMqttServerAsync(int id)
    {
        var mqttServer = await _mqttAppService.GetMqttServerByIdAsync(id); // 获取MQTT服务器信息用于内存删除
        var result = await _mqttAppService.DeleteMqttServerAsync(id) > 0;
        
        // 删除成功后，从内存中移除MQTT服务器
        if (result && mqttServer != null)
        {
            if (_appDataStorageService.MqttServers.TryRemove(id, out var mqttServerDto))
            {
                _eventService.RaiseMqttServerChanged(
                    this, new MqttServerChangedEventArgs(ActionChangeType.Deleted, mqttServerDto));
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步批量删除MQTT服务器。
    /// </summary>
    public async Task<bool> DeleteMqttServersAsync(List<int> ids)
    {
        var result = await _mqttAppService.DeleteMqttServersAsync(ids);
        
        // 批量删除成功后，从内存中移除MQTT服务器
        if (result && ids != null)
        {
            foreach (var id in ids)
            {
                if (_appDataStorageService.MqttServers.TryRemove(id, out var mqttServerDto))
                {
                    _eventService.RaiseMqttServerChanged(
                        this, new MqttServerChangedEventArgs(ActionChangeType.Deleted, mqttServerDto));
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// 获取发生变化的属性列表
    /// </summary>
    /// <param name="oldMqttServer">旧MQTT服务器值</param>
    /// <param name="newMqttServer">新MQTT服务器值</param>
    /// <returns>发生变化的属性列表</returns>
    private List<MqttServerPropertyType> GetChangedProperties(MqttServerDto oldMqttServer, MqttServerDto newMqttServer)
    {
        var changedProperties = new List<MqttServerPropertyType>();

        if (oldMqttServer.ServerName != newMqttServer.ServerName)
            changedProperties.Add(MqttServerPropertyType.ServerName);
        
        if (oldMqttServer.ServerUrl != newMqttServer.ServerUrl)
            changedProperties.Add(MqttServerPropertyType.ServerUrl);
        
        if (oldMqttServer.Port != newMqttServer.Port)
            changedProperties.Add(MqttServerPropertyType.Port);
        
        if (oldMqttServer.IsConnect != newMqttServer.IsConnect)
            changedProperties.Add(MqttServerPropertyType.IsConnect);
        
        if (oldMqttServer.Username != newMqttServer.Username)
            changedProperties.Add(MqttServerPropertyType.Username);
        
        if (oldMqttServer.Password != newMqttServer.Password)
            changedProperties.Add(MqttServerPropertyType.Password);
        
        if (oldMqttServer.IsActive != newMqttServer.IsActive)
            changedProperties.Add(MqttServerPropertyType.IsActive);
        
        if (oldMqttServer.SubscribeTopic != newMqttServer.SubscribeTopic)
            changedProperties.Add(MqttServerPropertyType.SubscribeTopic);
        
        if (oldMqttServer.PublishTopic != newMqttServer.PublishTopic)
            changedProperties.Add(MqttServerPropertyType.PublishTopic);
        
        if (oldMqttServer.ClientId != newMqttServer.ClientId)
            changedProperties.Add(MqttServerPropertyType.ClientId);
        
        if (oldMqttServer.MessageFormat != newMqttServer.MessageFormat)
            changedProperties.Add(MqttServerPropertyType.MessageFormat);
        
        if (oldMqttServer.MessageHeader != newMqttServer.MessageHeader)
            changedProperties.Add(MqttServerPropertyType.MessageHeader);
        
        if (oldMqttServer.MessageContent != newMqttServer.MessageContent)
            changedProperties.Add(MqttServerPropertyType.MessageContent);
        
        if (oldMqttServer.MessageFooter != newMqttServer.MessageFooter)
            changedProperties.Add(MqttServerPropertyType.MessageFooter);

        return changedProperties;
    }
}