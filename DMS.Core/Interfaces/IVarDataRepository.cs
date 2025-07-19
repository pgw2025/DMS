using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Core.Models;
using SqlSugar;

namespace DMS.Core.Interfaces
{
    public interface IVarDataRepository
    {
        Task<Variable> GetByIdAsync(int id);
        Task<Variable> GetByIdAsync(int id, SqlSugarClient db);
        Task<List<Variable>> GetAllAsync();
        Task<List<Variable>> GetAllAsync(SqlSugarClient db);
        Task<List<Variable>> GetByVariableTableIdAsync(int varTableId);
        Task<List<Variable>> GetByVariableTableIdAsync(int varTableId, SqlSugarClient db);
        Task<Variable> AddAsync(Variable variable);
        Task<Variable> AddAsync(Variable variable, SqlSugarClient db);
        Task<int> AddAsync(IEnumerable<Variable> variableDatas);
        Task<int> AddAsync(IEnumerable<Variable> variableDatas, SqlSugarClient db);
        Task<int> UpdateAsync(Variable variable);
        Task<int> UpdateAsync(Variable variable, SqlSugarClient db);
        Task<int> UpdateAsync(List<Variable> variableDatas);
        Task<int> UpdateAsync(List<Variable> variableDatas, SqlSugarClient db);
        Task<int> DeleteAsync(Variable variable);
        Task<int> DeleteAsync(Variable variable, SqlSugarClient db);
        Task<int> DeleteAsync(IEnumerable<Variable> variableDatas);
        
        Task<int> AddMqttToVariablesAsync(IEnumerable<VariableMqtt> variableMqttList);
    }
}