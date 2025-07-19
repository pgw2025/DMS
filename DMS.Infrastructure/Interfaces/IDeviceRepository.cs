using DMS.Core.Models;
using DMS.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface IDeviceRepository
    {
        Task<int> UpdateAsync(Device device);
        
        Task<List<Device>> GetAllAsync();
        Task<Device> GetByIdAsync(int id);
        Task<int> DeleteAsync(Device device, List<MenuBean> menus);
        
        Task AddAsync(Device device);
    }
}