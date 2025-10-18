using System.Collections.Concurrent;

using DMS.Application.Events;
using DMS.Application.Interfaces;
using DMS.Application.Interfaces.Database;
using DMS.Application.Interfaces.Management;
using DMS.Core.Enums;
using DMS.Core.Models;

namespace DMS.Application.Services.Management;

/// <summary>
/// MQTT管理服务，负责MQTT相关的业务逻辑。
/// </summary>
public class MqttManagementService : IMqttManagementService
{
    private readonly IMqttAppService _mqttAppService;
    private readonly IAppStorageService _appStorageService;
    private readonly IEventService _eventService;

    public MqttManagementService(IMqttAppService mqttAppService, 
                                IAppStorageService appStorageService, 
                                IEventService eventService)
    {
        _mqttAppService = mqttAppService;
        _appStorageService = appStorageService;
        _eventService = eventService;
    }

    /// <summary>
    /// 异步根据ID获取MQTT服务器DTO。
    /// </summary>
    public async Task<MqttServer> GetMqttServerByIdAsync(int id)
    {
        if (_appStorageService.MqttServers.TryGetValue(id,out var mqttServer))
        {
            return mqttServer;
        }
        return null;
    }

    /// <summary>
    /// 异步获取所有MQTT服务器DTO列表。
    /// </summary>
    public async Task<List<MqttServer>> GetAllMqttServersAsync()
    {
        return _appStorageService.MqttServers.Values.ToList();
    }

    /// <summary>
    /// 异步更新一个已存在的MQTT服务器。
    /// </summary>
    public async Task<int> UpdateMqttServerAsync(MqttServer mqttServer)
    {
        return await UpdateMqttServersAsync(new List<MqttServer>() { mqttServer });
    }

    /// <summary>
    /// 异步批量更新MQTT服务器。
    /// </summary>
    public async Task<int> UpdateMqttServersAsync(List<MqttServer> mqttServers)
    {
        var result = await _mqttAppService.UpdateMqttServersAsync(mqttServers);
        
        // 批量更新成功后，更新内存中的MQTT服务器
        if (result > 0 && mqttServers != null)
        {
            foreach (var mqttServer in mqttServers)
            {
                if (_appStorageService.MqttServers.TryGetValue(mqttServer.Id, out var mMqttServer))
                {
                    // 比较旧值和新值，确定哪个属性发生了变化
                    var changedProperties = GetChangedProperties(mMqttServer, mqttServer);
                    
                    // 更新内存中的MQTT服务器
                    mMqttServer.ServerName = mqttServer.ServerName;
                    mMqttServer.ServerUrl = mqttServer.ServerUrl;
                    mMqttServer.Port = mqttServer.Port;
                    mMqttServer.Username = mqttServer.Username;
                    mMqttServer.Password = mqttServer.Password;
                    mMqttServer.IsActive = mqttServer.IsActive;
                    mMqttServer.IsConnect = mqttServer.IsConnect;
                    mMqttServer.SubscribeTopic = mqttServer.SubscribeTopic;
                    mMqttServer.PublishTopic = mqttServer.PublishTopic;
                    mMqttServer.ClientId = mqttServer.ClientId;
                    mMqttServer.MessageFormat = mqttServer.MessageFormat;
                    mMqttServer.MessageHeader = mqttServer.MessageHeader;
                    mMqttServer.MessageContent = mqttServer.MessageContent;
                    mMqttServer.MessageFooter = mqttServer.MessageFooter;

                    // 为每个发生变化的属性触发事件
                    foreach (var property in changedProperties)
                    {
                        _eventService.RaiseMqttServerChanged(
                            this, new MqttServerChangedEventArgs(ActionChangeType.Updated, mMqttServer, property));
                    }
                    
                }
                else
                {
                    // 如果内存中不存在该MQTT服务器，则直接添加
                    _appStorageService.MqttServers.TryAdd(mqttServer.Id, mqttServer);
                    _eventService.RaiseMqttServerChanged(
                        this, new MqttServerChangedEventArgs(ActionChangeType.Added, mqttServer, MqttServerPropertyType.All));
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
            if (_appStorageService.MqttServers.TryRemove(id, out var mqttServerFromCache))
            {
                _eventService.RaiseMqttServerChanged(
                    this, new MqttServerChangedEventArgs(ActionChangeType.Deleted, mqttServerFromCache));
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
                if (_appStorageService.MqttServers.TryRemove(id, out var mqttServer))
                {
                    _eventService.RaiseMqttServerChanged(
                        this, new MqttServerChangedEventArgs(ActionChangeType.Deleted, mqttServer));
                }
            }
        }
        
        return result;
    }

    /// <summary>
    /// 异步创建MQTT服务器及其菜单项。
    /// </summary>
    public async Task<MqttServer> CreateMqttServerAsync(MqttServer mqttServer)
    {
        // 首先创建MQTT服务器
        var mqttServerId = await _mqttAppService.CreateMqttServerAsync(mqttServer);
        
        if (mqttServerId > 0)
        {
            mqttServer.Id = mqttServerId;
            
            
            // 将MQTT服务器添加到内存中
            if (_appStorageService.MqttServers.TryAdd(mqttServer.Id, mqttServer))
            {
                _eventService.RaiseMqttServerChanged(
                    this, new MqttServerChangedEventArgs(ActionChangeType.Added, mqttServer));
            }
            
            
        }

        return mqttServer; // 返回null表示创建失败
    }

    /// <summary>
    /// 获取发生变化的属性列表
    /// </summary>
    /// <param name="oldMqttServer">旧MQTT服务器值</param>
    /// <param name="newMqttServer">新MQTT服务器值</param>
    /// <returns>发生变化的属性列表</returns>
    private List<MqttServerPropertyType> GetChangedProperties(MqttServer oldMqttServer, MqttServer newMqttServer)
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