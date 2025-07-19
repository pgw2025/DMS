using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Core.Models;

namespace DMS.Infrastructure.Interfaces
{
    public interface IVarDataRepository
    {
        Task<Variable> GetByIdAsync(int id);
        Task<List<Variable>> GetAllAsync();
        Task<Variable> AddAsync(Variable variable);
        Task<int> UpdateAsync(Variable variable);
        Task<int> DeleteAsync(Variable variable);
        Task BeginTranAsync();
        Task CommitTranAsync();
        Task RollbackTranAsync();
    }
}