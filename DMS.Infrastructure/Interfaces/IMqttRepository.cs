using DMS.Core.Models;
using DMS.Infrastructure.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface IMqttRepository
    {
        Task<DbMqttServer> GetByIdAsync(int id);
        Task<List<DbMqttServer>> GetAllAsync();
        Task<int> AddAsync(DbMqttServer mqtt);
        Task<int> UpdateAsync(DbMqttServer mqtt);
        Task<int> DeleteAsync(DbMqttServer mqtt);
        Task BeginTranAsync();
        Task CommitTranAsync();
        Task RollbackTranAsync();
    }

}