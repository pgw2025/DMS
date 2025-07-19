using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface IVariableMqttAliasRepository
    {
        Task<VariableMqtt?> GetAliasByVariableAndMqtt(int variableDataId, int mqttId);
        Task<VariableMqtt?> GetAliasByVariableAndMqtt(int variableDataId, int mqttId, ITransaction db);
        Task<int> AddManyAsync(IEnumerable<VariableMqtt> entities);
        Task<int> AddManyAsync(IEnumerable<VariableMqtt> entities, ITransaction db);
        Task<int> UpdateAliasAsync(int variableDataId, int mqttId, string newAlias);
        Task<int> UpdateAliasAsync(int variableDataId, int mqttId, string newAlias, ITransaction db);
        Task<int> DeleteAsync(int variableDataId, int mqttId);
        
    }
}