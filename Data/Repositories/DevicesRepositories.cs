using PMSWPF.Data.Entities;
using PMSWPF.Excptions;

namespace PMSWPF.Data.Repositories;

public class DevicesRepositories:BaseRepositories
{
    public DevicesRepositories():base()
    {
       var tableExist= _db.DbMaintenance.IsAnyTable<DbDevice>();
       if (!tableExist)
       {
           _db.CodeFirst.InitTables<DbDevice>();
       }
    }
    
    public async Task<int> Add(DbDevice dbDevice)
    {
       var exist=await _db.Queryable<DbDevice>().Where(d=>d.Name==dbDevice.Name).FirstAsync();
       if (exist != null)
       {
           throw new DbExistException("设备名称已经存在。");
       }
      var res= await _db.Insertable<DbDevice>(dbDevice).ExecuteCommandAsync();
      
      return res;
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