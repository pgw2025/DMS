using System.Collections.ObjectModel;
using DMS.Application.DTOs;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.Interfaces;

/// <summary>
/// MQTT数据服务接口。
/// </summary>
public interface IMqttDataService
{


    /// <summary>
    /// 加载所有MQTT服务器数据。
    /// </summary>
    Task LoadMqttServers();

    /// <summary>
    /// 添加MQTT服务器。
    /// </summary>
    Task<MqttServerItem> AddMqttServer(MqttServerItem mqttServer);

    /// <summary>
    /// 更新MQTT服务器。
    /// </summary>
    Task<bool> UpdateMqttServer(MqttServerItem mqttServer);

    /// <summary>
    /// 删除MQTT服务器。
    /// </summary>
    Task<bool> DeleteMqttServer(MqttServerItem mqttServer);
}