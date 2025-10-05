using DMS.Application.DTOs;

namespace DMS.Application.Interfaces.Management;

public interface IMqttManagementService
{
    /// <summary>
    /// 异步根据ID获取MQTT服务器DTO。
    /// </summary>
    Task<MqttServerDto> GetMqttServerByIdAsync(int id);

    /// <summary>
    /// 异步获取所有MQTT服务器DTO列表。
    /// </summary>
    Task<List<MqttServerDto>> GetAllMqttServersAsync();

    /// <summary>
    /// 异步创建一个新的MQTT服务器。
    /// </summary>
    Task<MqttServerDto> CreateMqttServerAsync(MqttServerDto mqttServerDto);

    /// <summary>
    /// 异步更新一个已存在的MQTT服务器。
    /// </summary>
    Task<int> UpdateMqttServerAsync(MqttServerDto mqttServerDto);

    /// <summary>
    /// 异步批量更新MQTT服务器。
    /// </summary>
    Task<int> UpdateMqttServersAsync(List<MqttServerDto> mqttServerDtos);

    /// <summary>
    /// 异步删除一个MQTT服务器。
    /// </summary>
    Task<bool> DeleteMqttServerAsync(int id);

    /// <summary>
    /// 异步批量删除MQTT服务器。
    /// </summary>
    Task<bool> DeleteMqttServersAsync(List<int> ids);

}