using DMS.Application.DTOs;

namespace DMS.Application.Interfaces;

/// <summary>
/// 定义MQTT别名管理相关的应用服务操作。
/// </summary>
public interface IMqttAliasAppService
{
    /// <summary>
    /// 异步根据ID获取MQTT别名DTO。
    /// </summary>
    Task<VariableMqttAliasDto> GetMqttAliasByIdAsync(int id);

    /// <summary>
    /// 异步获取所有MQTT别名DTO列表。
    /// </summary>
    Task<List<VariableMqttAliasDto>> GetAllMqttAliasesAsync();

    /// <summary>
    /// 异步创建一个新的MQTT别名。
    /// </summary>
    Task<int> CreateMqttAliasAsync(VariableMqttAliasDto mqttAliasDto);

    /// <summary>
    /// 异步更新一个已存在的MQTT别名。
    /// </summary>
    Task UpdateMqttAliasAsync(VariableMqttAliasDto mqttAliasDto);

    /// <summary>
    /// 异步删除一个MQTT别名。
    /// </summary>
    Task DeleteMqttAliasAsync(int id);
}