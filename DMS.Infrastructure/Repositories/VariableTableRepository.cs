using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

public class VariableTableRepository : BaseRepository<DbVariableTable>, IVariableTableRepository
{
    public VariableTableRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
    }

    public async Task<VariableTable> GetByIdAsync(int id) => throw new NotImplementedException();

    public async Task<List<VariableTable>> GetAllAsync() => throw new NotImplementedException();

    public async Task<VariableTable> AddAsync(VariableTable entity) => throw new NotImplementedException();

    public async Task<int> UpdateAsync(VariableTable entity) => throw new NotImplementedException();

    public async Task<int> DeleteAsync(VariableTable entity) => throw new NotImplementedException();
}