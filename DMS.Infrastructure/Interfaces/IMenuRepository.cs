using DMS.Core.Models;
using DMS.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Interfaces
{
    public interface IMenuRepository
    {
        Task<int> DeleteAsync(DbMenu menu);
        Task<List<DbMenu>> GetMenuTreesAsync();
        Task<DbMenu> AddAsync(DbMenu menu);
        Task<int> UpdateAsync(DbMenu menu);
        Task BeginTranAsync();
        Task CommitTranAsync();
        Task RollbackTranAsync();
    }
}