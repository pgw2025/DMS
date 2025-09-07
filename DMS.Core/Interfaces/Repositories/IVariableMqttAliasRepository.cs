
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Core.Interfaces.Repositories
{
    public interface IVariableMqttAliasRepository : IBaseRepository<VariableMqttAlias>
    {
        /// <summary>
        /// 异步获取指定变量的所有MQTT别名关联。
        /// </summary>
        Task<List<VariableMqttAlias>> GetAliasesForVariableAsync(int variableId);

        /// <summary>
        /// 异步根据变量和服务器获取别名关联。
        /// </summary>
        Task<VariableMqttAlias> GetByVariableAndServerAsync(int variableId, int mqttServerId);

        /// <summary>
        /// 异步获取所有变量与MQTT别名关联。
        /// </summary>
        /// <returns>包含所有变量与MQTT别名关联实体的列表。</returns>
        Task<List<VariableMqttAlias>> GetAllAsync();
    }
}