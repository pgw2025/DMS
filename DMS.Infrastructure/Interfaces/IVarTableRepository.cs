using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Core.Models;

namespace DMS.Infrastructure.Interfaces
{
    public interface IVarTableRepository
    {
        Task<VariableTable> AddAsync(VariableTable varTable);
        Task<VariableTable> AddAsync(VariableTable variableTable, ITransaction db);
        Task<int> UpdateAsync(VariableTable variableTable);
        Task<int> UpdateAsync(VariableTable variableTable, ITransaction db);
        Task<int> DeleteAsync(VariableTable variableTable);
        Task<int> DeleteAsync(VariableTable varTable, ITransaction db);
        
    }
}