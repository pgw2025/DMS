using DMS.Core.Models;
using SqlSugar;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface IDeviceRepository
    {
        Task<DbDevice> AddAsync(DbDevice model);
        Task<int> UpdateAsync(DbDevice model);
        Task<int> DeleteAsync(DbDevice model);
        Task<List<DbDevice>> GetAllAsync();
        Task<DbDevice> GetByIdAsync(int id);
        Task BeginTranAsync();
        Task CommitTranAsync();
        Task RollbackTranAsync();
    }
}