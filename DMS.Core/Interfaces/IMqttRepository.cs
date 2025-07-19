using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Core.Interfaces
{
    public interface IMqttRepository
    {
        Task<Mqtt> GetByIdAsync(int id);
        Task<List<Mqtt>> GetAllAsync();
        Task<int> AddAsync(Mqtt mqtt);
        Task<int> UpdateAsync(Mqtt mqtt);
        Task<int> DeleteAsync(Mqtt mqtt);
    }
}