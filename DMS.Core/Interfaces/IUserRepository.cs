using DMS.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Core.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<List<User>> GetAllAsync();
        Task<int> AddAsync(User user);
        Task<int> UpdateAsync(User user);
        Task<int> DeleteAsync(int id);
    }
}