using System.Collections.ObjectModel;
using DMS.Application.DTOs;
using DMS.WPF.ItemViewModel;

namespace DMS.WPF.Interfaces;

/// <summary>
/// MQTT别名数据服务接口。
/// </summary>
public interface IMqttAliasDataService
{
    /// <summary>
    /// 加载所有MQTT别名数据。
    /// </summary>
    Task LoadMqttAliases();

    /// <summary>
    /// 添加MQTT别名。
    /// </summary>
    Task<MqttAliasItem> AssignAliasAsync(MqttAliasItem mqttAlias);

    /// <summary>
    /// 更新MQTT别名。
    /// </summary>
    Task<bool> UpdateMqttAlias(MqttAliasItem mqttAlias);

    /// <summary>
    /// 删除MQTT别名。
    /// </summary>
    Task<bool> DeleteMqttAlias(MqttAliasItem mqttAlias);

    /// <summary>
    /// 根据ID获取MQTT别名。
    /// </summary>
    Task<MqttAliasItem> GetMqttAliasById(int id);
}