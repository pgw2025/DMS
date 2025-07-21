using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

/// <summary>
///     用户仓储类，用于操作DbUser实体
/// </summary>
public class UserRepository : BaseRepository<DbUser>, IUserRepository
{
    public UserRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<User> GetByIdAsync(int id) => throw new NotImplementedException();

    public async Task<List<User>> GetAllAsync() => throw new NotImplementedException();

    public async Task<User> AddAsync(User entity) => throw new NotImplementedException();

    public async Task<int> UpdateAsync(User entity) => throw new NotImplementedException();

    public async Task<int> DeleteAsync(User entity) => throw new NotImplementedException();
}