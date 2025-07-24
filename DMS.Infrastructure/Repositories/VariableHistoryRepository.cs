using System.Diagnostics;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;


using AutoMapper;
using DMS.Core.Helper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

public class VariableHistoryRepository : BaseRepository<DbVariableHistory>, IVariableHistoryRepository
{
    private readonly IMapper _mapper;

    public VariableHistoryRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
    {
        _mapper = mapper;
    }

    public async Task<VariableHistory> GetByIdAsync(int id)
    {
        var dbVariableHistory = await base.GetByIdAsync(id);
        return _mapper.Map<VariableHistory>(dbVariableHistory);
    }

    public async Task<List<VariableHistory>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<VariableHistory>>(dbList);
    }

    public async Task<VariableHistory> AddAsync(VariableHistory entity)
    {
        var dbVariableHistory = await base.AddAsync(_mapper.Map<DbVariableHistory>(entity));
        return _mapper.Map(dbVariableHistory, entity);
    }

    public async Task<int> UpdateAsync(VariableHistory entity) => await base.UpdateAsync(_mapper.Map<DbVariableHistory>(entity));

    public async Task<int> DeleteAsync(VariableHistory entity) => await base.DeleteAsync(_mapper.Map<DbVariableHistory>(entity));
    
    public async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new Variable() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbMenu)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    
    public new async Task<List<VariableHistory>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<VariableHistory>>(dbList);

    }
}