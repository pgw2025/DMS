using PMSWPF.Data.Entities;
using PMSWPF.Extensions;
using PMSWPF.Models;
using SqlSugar;

namespace PMSWPF.Data.Repositories;

public class DeviceRepository 
{
    public DeviceRepository()
    {
        
    }
    /// <summary>
    /// 添加设备
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<Device> Add(Device device)
    {
        using (var db=DbContext.GetInstance())
        {
            var exist = await db.Queryable<DbDevice>().Where(d => d.Name == device.Name).FirstAsync();
            if (exist != null) 
                throw new InvalidOperationException("设备名称已经存在。");
            var dbDevice = new DbDevice();
            device.CopyTo(dbDevice);
            dbDevice.VariableTables = new List<DbVariableTable>();
            // 添加默认变量表
            var dbVariableTable = new DbVariableTable();
            dbVariableTable.Name = "默认变量表";
            dbVariableTable.Description = "默认变量表";
            dbVariableTable.ProtocolType = dbDevice.ProtocolType;
            dbDevice.VariableTables.Add(dbVariableTable);
            var addDbDevice= await db.InsertNav(dbDevice).Include(d => d.VariableTables).ExecuteReturnEntityAsync();
            return addDbDevice.CopyTo<Device>();
        }
       
    }

    public async Task<int> Edit(Device device)
    {
        using (var db=DbContext.GetInstance())
        {
            return await db.Updateable<DbDevice>(device.CopyTo<DbDevice>()).ExecuteCommandAsync();
        }
        
    }
    

    public async Task<List<Device>> GetAll()
    {
        using (var db = DbContext.GetInstance())
        {
            var dlist = await db.Queryable<DbDevice>().Includes(d => d.VariableTables,dv=>dv.Device).ToListAsync();
            var devices = new List<Device>();
            foreach (var dbDevice in dlist)
            {
                var device = dbDevice.CopyTo<Device>();
                devices.Add(device);
            }

            return devices;
        }
       
    }

    public async Task<DbDevice> GetById(int id)
    {
        using (var db=DbContext.GetInstance())
        {
            return await db.Queryable<DbDevice>().FirstAsync(p => p.Id == id);
        }
        
    }

    public async Task<int> DeleteById(int id)
    {
        using (var db = DbContext.GetInstance())
        {
            return await db.Deleteable<DbDevice>(new DbDevice { Id = id }).ExecuteCommandAsync();
        }
        
    }
}