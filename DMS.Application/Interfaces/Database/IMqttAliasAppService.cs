using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Application.Interfaces.Database;

/// <summary>
/// 定义了MQTT别名管理相关的应用服务操作。
/// </summary>
public interface IMqttAliasAppService
{
    /// <summary>
    /// 异步获取指定变量的所有MQTT别名关联。
    /// </summary>
    Task<List<VariableMqttAlias>> GetAliasesForVariableAsync(int variableId);

    /// <summary>
    /// 异步为变量分配或更新一个MQTT别名。
    /// </summary>
    /// <param name="variableId">变量ID。</param>
    /// <param name="mqttServerId">MQTT服务器ID。</param>
    /// <param name="alias">要设置的别名。</param>
    Task AssignAliasAsync(int variableId, int mqttServerId, string alias);

    /// <summary>
    /// 异步更新一个已存在的MQTT别名。
    /// </summary>
    /// <param name="aliasId">别名关联的ID。</param>
    /// <param name="newAlias">新的别名字符串。</param>
    Task UpdateAliasAsync(int aliasId, string newAlias);

    /// <summary>
    /// 异步移除一个MQTT别名关联。
    /// </summary>
    /// <param name="aliasId">要移除的别名关联的ID。</param>
    Task RemoveAliasAsync(int aliasId);
}