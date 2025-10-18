using DMS.Core.Models;

namespace DMS.Application.Interfaces.Management;

public interface IMqttManagementService
{
    Task<MqttServer> CreateMqttServerAsync(MqttServer mqttServer);

    /// <summary>
    /// 异步删除一个MQTT服务器。
    /// </summary>
    Task<bool> DeleteMqttServerAsync(int id);

    /// <summary>
    /// 异步批量删除MQTT服务器。
    /// </summary>
    Task<bool> DeleteMqttServersAsync(List<int> ids);
    Task<List<MqttServer>> GetAllMqttServersAsync();
    Task<MqttServer> GetMqttServerByIdAsync(int id);
    Task<int> UpdateMqttServerAsync(MqttServer mqttServer);
    Task<int> UpdateMqttServersAsync(List<MqttServer> mqttServers);

    /// <summary>
    /// 异步加载所有MQTT服务器数据到内存中。
    /// </summary>
    Task LoadAllMqttServersAsync();
}