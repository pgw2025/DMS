using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface IDeviceService
    {
        Task<Device> AddAsync(Device device);
        Task<List<Device>> GetAllAsync();
    }
}