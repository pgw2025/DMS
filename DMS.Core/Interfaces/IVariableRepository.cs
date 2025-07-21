using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IVariableRepository : IBaseRepository<Variable>
{
    /// <summary>
    /// 异步获取一个变量及其关联的所有MQTT别名和对应的MQTT服务器信息。
    /// </summary>
    /// <param name="variableId">变量ID。</param>
    /// <returns>包含别名和服务器信息的变量对象。</returns>
    Task<Variable> GetVariableWithMqttAliasesAsync(int variableId);
}