using PMSWPF.Data.Entities;

namespace PMSWPF.Data.Repositories;

public class DevicesRepositories:BaseRepositories
{
    public DevicesRepositories()
    {
        
    }
    
    public async Task<int> Add(DbDevice dbDevice)
    {
      return await _db.Insertable<DbDevice>(dbDevice).ExecuteCommandAsync();
    }
    
    public async Task<List<DbDevice>> GetAll()
    {
        return await _db.Queryable<DbDevice>().ToListAsync();
    }
    public async Task<DbDevice> GetById(int id)
    {
        return await _db.Queryable<DbDevice>().FirstAsync(p=>p.Id == id);
    }
    public async Task<int> DeleteById(int id)
    {
        return await _db.Deleteable<DbDevice>(new DbDevice() { Id = id }).ExecuteCommandAsync();
    }
}