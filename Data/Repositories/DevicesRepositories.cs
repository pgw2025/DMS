using PMSWPF.Data.Entities;
using PMSWPF.Enums;
using PMSWPF.Excptions;
using PMSWPF.Extensions;
using PMSWPF.Models;

namespace PMSWPF.Data.Repositories;

public class DevicesRepositories:BaseRepositories
{
    public DevicesRepositories():base()
    {
     
    }
    
    public async Task<bool> Add(Device device)
    {
       var exist=await _db.Queryable<DbDevice>().Where(d=>d.Name==device.Name).FirstAsync();
       if (exist != null)
       {
           throw new DbExistException("设备名称已经存在。");
       }
       DbDevice dbDevice=new DbDevice();
       device.CopyTo<DbDevice>(dbDevice);
       dbDevice.VariableTables=new ();
       // 添加默认变量表
       DbVariableTable dbVariableTable=new DbVariableTable();
       dbVariableTable.Name = "默认变量表";
       dbVariableTable.Description = "默认变量表";
       dbVariableTable.ProtocolType = dbDevice.ProtocolType;
       dbDevice.VariableTables.Add(dbVariableTable);
      return await _db.InsertNav(dbDevice).Include(d=>d.VariableTables).ExecuteCommandAsync();

    }
    
    public async Task<List<DbDevice>> GetAll()
    {
        return await _db.Queryable<DbDevice>().Includes(d=>d.VariableTables).ToListAsync();
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