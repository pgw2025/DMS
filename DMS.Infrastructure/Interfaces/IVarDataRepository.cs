using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Core.Models;

namespace DMS.Infrastructure.Interfaces
{
    public interface IVarDataRepository
    {
        Task<Variable> GetByIdAsync(int id);
        Task<Variable> GetByIdAsync(int id, ITransaction db);
        Task<List<Variable>> GetAllAsync();
        Task<List<Variable>> GetAllAsync(ITransaction db);
        Task<List<Variable>> GetByVariableTableIdAsync(int varTableId);
        Task<List<Variable>> GetByVariableTableIdAsync(int varTableId, ITransaction db);
        Task<Variable> AddAsync(Variable variable);
        Task<Variable> AddAsync(Variable variable, ITransaction db);
        Task<int> AddAsync(IEnumerable<Variable> variableDatas);
        Task<int> AddAsync(IEnumerable<Variable> variableDatas, ITransaction db);
        Task<int> UpdateAsync(Variable variable);
        Task<int> UpdateAsync(Variable variable, ITransaction db);
        Task<int> UpdateAsync(List<Variable> variableDatas);
        Task<int> UpdateAsync(List<Variable> variableDatas, ITransaction db);
        Task<int> DeleteAsync(Variable variable);
        Task<int> DeleteAsync(Variable variable, ITransaction db);
        Task<int> DeleteAsync(IEnumerable<Variable> variableDatas);
        
        Task<int> AddMqttToVariablesAsync(IEnumerable<VariableMqtt> variableMqttList);
    }
}