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
    Task<int> CreateMqttServerAsync(MqttServerDto mqttServerDto);

    /// <summary>
    /// 异步更新一个已存在的MQTT服务器。
    /// </summary>
    Task UpdateMqttServerAsync(MqttServerDto mqttServerDto);

    /// <summary>
    /// 异步删除一个MQTT服务器。
    /// </summary>
    Task DeleteMqttServerAsync(int id);
}