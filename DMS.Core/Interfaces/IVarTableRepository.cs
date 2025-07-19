using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Core.Models;

namespace DMS.Core.Interfaces
{
    public interface IVarTableRepository
    {
        Task<VariableTable> AddAsync(VariableTable varTable);
        Task<VariableTable> AddAsync(VariableTable variableTable, SqlSugarClient db);
        Task<int> UpdateAsync(VariableTable variableTable);
        Task<int> UpdateAsync(VariableTable variableTable, SqlSugarClient db);
        Task<int> DeleteAsync(VariableTable variableTable);
        Task<int> DeleteAsync(VariableTable varTable, SqlSugarClient db);
        
    }
}