using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

public class VariableHistoryRepository : BaseRepository<DbVariableHistory>, IVariableHistoryRepository
{
    public VariableHistoryRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<VariableHistory> GetByIdAsync(int id) => throw new NotImplementedException();

    public async Task<List<VariableHistory>> GetAllAsync() => throw new NotImplementedException();

    public async Task<VariableHistory> AddAsync(VariableHistory entity) => throw new NotImplementedException();

    public async Task<int> UpdateAsync(VariableHistory entity) => throw new NotImplementedException();

    public async Task<int> DeleteAsync(VariableHistory entity) => throw new NotImplementedException();
}