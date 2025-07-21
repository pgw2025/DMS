using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IVariableMqttAliasRepository : IBaseRepository<VariableMqttAlias>
{
    /// <summary>
    /// 异步获取指定变量的所有MQTT别名关联，并加载关联的MQTT服务器信息。
    /// </summary>
    /// <param name="variableId">变量ID。</param>
    /// <returns>指定变量的所有MQTT别名关联列表。</returns>
    Task<List<VariableMqttAlias>> GetAliasesForVariableAsync(int variableId);

    /// <summary>
    /// 异步根据变量ID和MQTT服务器ID获取特定的MQTT别名关联。
    /// </summary>
    /// <param name="variableId">变量ID。</param>
    /// <param name="mqttServerId">MQTT服务器ID。</param>
    /// <returns>匹配的VariableMqttAlias对象，如果不存在则为null。</returns>
    Task<VariableMqttAlias> GetByVariableAndServerAsync(int variableId, int mqttServerId);
}