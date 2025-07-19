using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface IDeviceService
    {
        Task<int> DeleteAsync(Device device, List<MenuBean> menus);
        Task AddAsync(Device device);
    }
}