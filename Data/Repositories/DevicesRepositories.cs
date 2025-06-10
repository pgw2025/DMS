using PMSWPF.Data.Entities;

namespace PMSWPF.Data.Repositories;

public class DevicesRepositories:BaseRepositories
{
    public DevicesRepositories()
    {
        
    }
    
    public async Task<int> Add(DbPLC dbPLC)
    {
      return await _db.Insertable<DbPLC>(dbPLC).ExecuteCommandAsync();
    }
    
    public async Task<List<DbPLC>> GetAll()
    {
        return await _db.Queryable<DbPLC>().ToListAsync();
    }
    public async Task<DbPLC> GetById(int id)
    {
        return await _db.Queryable<DbPLC>().FirstAsync(p=>p.id == id);
    }
    public async Task<int> DeleteById(int id)
    {
        return await _db.Deleteable<DbPLC>(new DbPLC() { id = id }).ExecuteCommandAsync();
    }
}