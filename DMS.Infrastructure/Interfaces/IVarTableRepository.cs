using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Core.Models;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Interfaces
{
    public interface IVarTableRepository
    {
        Task<DbVariableTable> AddAsync(DbVariableTable varTable);
        Task<int> UpdateAsync(DbVariableTable variableTable);
        Task<int> DeleteAsync(DbVariableTable variableTable);
        Task<List<DbVariableTable>> GetAllAsync();
        Task<DbVariableTable> GetByIdAsync(int id);

    }
}