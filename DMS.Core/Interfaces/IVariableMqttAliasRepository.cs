using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Core.Interfaces
{
    public interface IVariableMqttAliasRepository
    {
        Task<VariableMqtt?> GetAliasByVariableAndMqtt(int variableDataId, int mqttId);
        Task<VariableMqtt?> GetAliasByVariableAndMqtt(int variableDataId, int mqttId, SqlSugarClient db);
        Task<int> AddManyAsync(IEnumerable<VariableMqtt> entities);
        Task<int> AddManyAsync(IEnumerable<VariableMqtt> entities, SqlSugarClient db);
        Task<int> UpdateAliasAsync(int variableDataId, int mqttId, string newAlias);
        Task<int> UpdateAliasAsync(int variableDataId, int mqttId, string newAlias, SqlSugarClient db);
        Task<int> DeleteAsync(int variableDataId, int mqttId);
        
    }
}