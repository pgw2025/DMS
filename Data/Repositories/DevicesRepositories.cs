using PMSWPF.Data.Entities;
using PMSWPF.Excptions;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Models;
using SqlSugar;

namespace PMSWPF.Data.Repositories;

public class DevicesRepositories 
{
    private SqlSugarClient _db;
    public DevicesRepositories()
    {
        _db = DbContext.GetInstance();
    }
    public async Task<bool> Add(Device device)
    {
        var exist = await _db.Queryable<DbDevice>().Where(d => d.Name == device.Name).FirstAsync();
        if (exist != null) throw new DbExistException("设备名称已经存在。");
        var dbDevice = new DbDevice();
        device.CopyTo(dbDevice);
        dbDevice.VariableTables = new List<DbVariableTable>();
        // 添加默认变量表
        var dbVariableTable = new DbVariableTable();
        dbVariableTable.Name = "默认变量表";
        dbVariableTable.Description = "默认变量表";
        dbVariableTable.ProtocolType = dbDevice.ProtocolType;
        dbDevice.VariableTables.Add(dbVariableTable);
        return await _db.InsertNav(dbDevice).Include(d => d.VariableTables).ExecuteCommandAsync();
    }

    public async Task<List<Device>> GetAll()
    {
        var dlist = await _db.Queryable<DbDevice>().Includes(d => d.VariableTables).ToListAsync();
        var devices = new List<Device>();
        foreach (var dbDevice in dlist)
        {
            var device = dbDevice.CopyTo<Device>();
            devices.Add(device);
        }

        return devices;
    }

    public async Task<DbDevice> GetById(int id)
    {
        return await _db.Queryable<DbDevice>().FirstAsync(p => p.Id == id);
    }

    public async Task<int> DeleteById(int id)
    {
        return await _db.Deleteable<DbDevice>(new DbDevice { Id = id }).ExecuteCommandAsync();
    }
}