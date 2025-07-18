using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Data.Entities;

namespace DMS.Data.Repositories;

/// <summary>
/// 变量与MQTT服务器别名关联的数据仓库。
/// </summary>
public class VariableMqttAliasRepository
{
    /// <summary>
    /// 根据变量ID和MQTT服务器ID获取别名。
    /// </summary>
    /// <param name="variableDataId">变量数据ID。</param>
    /// <param name="mqttId">MQTT服务器ID。</param>
    /// <returns>DbVariableMqtt实体，如果不存在则为null。</returns>
    public async Task<DbVariableMqtt?> GetAliasByVariableAndMqtt(int variableDataId, int mqttId)
    {
        using (var db = DbContext.GetInstance())
        {
            return await GetAliasByVariableAndMqtt(variableDataId, mqttId, db);
        }
    }

    /// <summary>
    /// 根据变量ID和MQTT服务器ID获取别名。
    /// </summary>
    /// <param name="variableDataId">变量数据ID。</param>
    /// <param name="mqttId">MQTT服务器ID。</param>
    /// <param name="db">SqlSugarClient实例。</param>
    /// <returns>DbVariableMqtt实体，如果不存在则为null。</returns>
    public async Task<DbVariableMqtt?> GetAliasByVariableAndMqtt(int variableDataId, int mqttId, SqlSugarClient db)
    {
        return await db.Queryable<DbVariableMqtt>()
                            .Where(it => it.VariableId == variableDataId && it.MqttId == mqttId)
                            .FirstAsync();
    }

    /// <summary>
    /// 批量添加变量与MQTT服务器的关联。
    /// </summary>
    /// <param name="entities">要添加的DbVariableMqtt实体列表。</param>
    /// <returns>成功添加的数量。</returns>
    public async Task<int> AddManyAsync(IEnumerable<DbVariableMqtt> entities)
    {
        using (var db = DbContext.GetInstance())
        {
            return await AddManyAsync(entities, db);
        }
    }

    /// <summary>
    /// 批量添加变量与MQTT服务器的关联。
    /// </summary>
    /// <param name="entities">要添加的DbVariableMqtt实体列表。</param>
    /// <param name="db">SqlSugarClient实例。</param>
    /// <returns>成功添加的数量。</returns>
    public async Task<int> AddManyAsync(IEnumerable<DbVariableMqtt> entities, SqlSugarClient db)
    {
        return await db.Insertable<DbVariableMqtt>(entities).ExecuteCommandAsync();
    }

    /// <summary>
    /// 更新变量与MQTT服务器的别名。
    /// </summary>
    /// <param name="variableDataId">变量数据ID。</param>
    /// <param name="mqttId">MQTT服务器ID。</param>
    /// <param name="newAlias">新的别名。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAliasAsync(int variableDataId, int mqttId, string newAlias)
    {
        using (var db = DbContext.GetInstance())
        {
            return await UpdateAliasAsync(variableDataId, mqttId, newAlias, db);
        }
    }

    /// <summary>
    /// 更新变量与MQTT服务器的别名。
    /// </summary>
    /// <param name="variableDataId">变量数据ID。</param>
    /// <param name="mqttId">MQTT服务器ID。</param>
    /// <param name="newAlias">新的别名。</param>
    /// <param name="db">SqlSugarClient实例。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAliasAsync(int variableDataId, int mqttId, string newAlias, SqlSugarClient db)
    {
        return await db.Updateable<DbVariableMqtt>()
                            .SetColumns(it => it.MqttAlias == newAlias)
                            .Where(it => it.VariableId == variableDataId && it.MqttId == mqttId)
                            .ExecuteCommandAsync();
    }

    /// <summary>
    /// 删除变量与MQTT服务器的关联。
    /// </summary>
    /// <param name="variableDataId">变量数据ID。</param>
    /// <param name="mqttId">MQTT服务器ID。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(int variableDataId, int mqttId)
    {
        using (var db = DbContext.GetInstance())
        {
            return await DeleteAsync(variableDataId, mqttId, db);
        }
    }

    /// <summary>
    /// 删除变量与MQTT服务器的关联。
    /// </summary>
    /// <param name="variableDataId">变量数据ID。</param>
    /// <param name="mqttId">MQTT服务器ID。</param>
    /// <param name="db">SqlSugarClient实例。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(int variableDataId, int mqttId, SqlSugarClient db)
    {
        return await db.Deleteable<DbVariableMqtt>()
                            .Where(it => it.VariableId == variableDataId && it.MqttId == mqttId)
                            .ExecuteCommandAsync();
    }
}
