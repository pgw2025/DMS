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

    /// <summary>
    /// 当MQTT服务器数据发生变化时触发
    /// </summary>
    public event EventHandler<MqttServerChangedEventArgs> MqttServerChanged;

    public MqttManagementService(IMqttAppService mqttAppService,IAppDataStorageService appDataStorageService)
    {
        _mqttAppService = mqttAppService;
        _appDataStorageService = appDataStorageService;
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
        return await _mqttAppService.CreateMqttServerAsync(mqttServerDto);
    }

    /// <summary>
    /// 异步更新一个已存在的MQTT服务器。
    /// </summary>
    public async Task UpdateMqttServerAsync(MqttServerDto mqttServerDto)
    {
        await _mqttAppService.UpdateMqttServerAsync(mqttServerDto);
    }

    /// <summary>
    /// 异步删除一个MQTT服务器。
    /// </summary>
    public async Task DeleteMqttServerAsync(int id)
    {
        await _mqttAppService.DeleteMqttServerAsync(id);
    }

    /// <summary>
    /// 在内存中添加MQTT服务器
    /// </summary>
    public void AddMqttServerToMemory(MqttServerDto mqttServerDto)
    {
        if (_appDataStorageService.MqttServers.TryAdd(mqttServerDto.Id, mqttServerDto))
        {
            OnMqttServerChanged(new MqttServerChangedEventArgs(DataChangeType.Added, mqttServerDto));
        }
    }

    /// <summary>
    /// 在内存中更新MQTT服务器
    /// </summary>
    public void UpdateMqttServerInMemory(MqttServerDto mqttServerDto)
    {
        _appDataStorageService.MqttServers.AddOrUpdate(mqttServerDto.Id, mqttServerDto, (key, oldValue) => mqttServerDto);
        OnMqttServerChanged(new MqttServerChangedEventArgs(DataChangeType.Updated, mqttServerDto));
    }

    /// <summary>
    /// 在内存中删除MQTT服务器
    /// </summary>
    public void RemoveMqttServerFromMemory(int mqttServerId)
    {
        if (_appDataStorageService.MqttServers.TryRemove(mqttServerId, out var mqttServerDto))
        {
            OnMqttServerChanged(new MqttServerChangedEventArgs(DataChangeType.Deleted, mqttServerDto));
        }
    }

    /// <summary>
    /// 触发MQTT服务器变更事件
    /// </summary>
    protected virtual void OnMqttServerChanged(MqttServerChangedEventArgs e)
    {
        MqttServerChanged?.Invoke(this, e);
    }
}