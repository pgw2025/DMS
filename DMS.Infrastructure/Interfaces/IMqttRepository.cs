using DMS.Core.Models;
using DMS.Infrastructure.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface IMqttRepository
    {
        Task<DbMqtt> GetByIdAsync(int id);
        Task<List<DbMqtt>> GetAllAsync();
        Task<int> AddAsync(DbMqtt mqtt);
        Task<int> UpdateAsync(DbMqtt mqtt);
        Task<int> DeleteAsync(DbMqtt mqtt);
        Task BeginTranAsync();
        Task CommitTranAsync();
        Task RollbackTranAsync();
    }
}