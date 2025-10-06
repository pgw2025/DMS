using DMS.Core.Models;

namespace DMS.Application.Interfaces.Database;

/// <summary>
/// 定义MQTT服务器管理相关的应用服务操作。
/// </summary>
public interface IMqttAppService
{
    /// <summary>
    /// 异步根据ID获取MQTT服务器。
    /// </summary>
    Task<MqttServer> GetMqttServerByIdAsync(int id);

    /// <summary>
    /// 异步获取所有MQTT服务器列表。
    /// </summary>
    Task<List<MqttServer>> GetAllMqttServersAsync();

    /// <summary>
    /// 异步创建一个新的MQTT服务器。
    /// </summary>
    Task<int> CreateMqttServerAsync(MqttServer mqttServer);

    /// <summary>
    /// 异步更新一个已存在的MQTT服务器。
    /// </summary>
    Task UpdateMqttServerAsync(MqttServer mqttServer);

    /// <summary>
    /// 异步批量更新MQTT服务器。
    /// </summary>
    Task<int> UpdateMqttServersAsync(List<MqttServer> mqttServers);

    /// <summary>
    /// 异步根据ID删除一个MQTT服务器。
    /// </summary>
    Task<int> DeleteMqttServerAsync(int id);

    /// <summary>
    /// 异步批量删除MQTT服务器。
    /// </summary>
    Task<bool> DeleteMqttServersAsync(List<int> ids);
}