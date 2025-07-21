using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IMqttServerRepository : IBaseRepository<MqttServer>
{
    /// <summary>
    /// 异步获取一个MQTT服务器及其关联的所有变量别名。
    /// </summary>
    /// <param name="serverId">MQTT服务器ID。</param>
    /// <returns>包含变量别名信息的MQTT服务器对象。</returns>
    Task<MqttServer> GetMqttServerWithVariableAliasesAsync(int serverId);
}