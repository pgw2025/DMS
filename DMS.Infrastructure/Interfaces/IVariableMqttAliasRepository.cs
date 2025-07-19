using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface IVariableMqttAliasRepository
    {
        Task<VariableMqtt?> GetByIdAsync(int variableDataId, int mqttId);
        Task<int> UpdateAliasAsync(int variableDataId, int mqttId, string newAlias);
        Task<int> DeleteAsync(int variableDataId, int mqttId);
        Task BeginTranAsync();
        Task CommitTranAsync();
        Task RollbackTranAsync();

    }
}