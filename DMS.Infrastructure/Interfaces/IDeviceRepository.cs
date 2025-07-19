using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface IDeviceRepository
    {
        Task<int> AddAsync(Device model);
        Task<int> UpdateAsync(Device model);
        Task<int> DeleteAsync(Device model);
        Task<List<Device>> GetAllAsync();
        Task<Device> GetByIdAsync(int id);
    }
}