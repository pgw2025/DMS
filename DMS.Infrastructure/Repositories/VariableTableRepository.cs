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

public class VariableTableRepository : BaseRepository<DbVariableTable>, IVariableTableRepository
{
    private readonly IMapper _mapper;

    public VariableTableRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
    {
        _mapper = mapper;
    }

    public async Task<VariableTable> GetByIdAsync(int id)
    {
        var dbVariableTable = await base.GetByIdAsync(id);
        return _mapper.Map<VariableTable>(dbVariableTable);
    }

    public async Task<List<VariableTable>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<VariableTable>>(dbList);
    }

    public async Task<VariableTable> AddAsync(VariableTable entity)
    {
        var dbVariableTable = await base.AddAsync(_mapper.Map<DbVariableTable>(entity));
        return _mapper.Map(dbVariableTable, entity);
    }

    public async Task<int> UpdateAsync(VariableTable entity) => await base.UpdateAsync(_mapper.Map<DbVariableTable>(entity));

    public async Task<int> DeleteAsync(VariableTable entity) => await base.DeleteAsync(_mapper.Map<DbVariableTable>(entity));
    
    public async Task<int> DeleteAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new VariableTable() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbMenu)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    public new async Task<List<VariableTable>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<VariableTable>>(dbList);

    }
}