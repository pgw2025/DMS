using PMSWPF.Data.Entities;
using PMSWPF.Extensions;
using PMSWPF.Models;
using SqlSugar;

namespace PMSWPF.Data.Repositories;

public class VariableTableRepository
{
    private SqlSugarClient _db;
    public VariableTableRepository()
    {
        _db = DbContext.GetInstance();
    }
    // public async Task<VariableTable> Add(VariableTable variableTable)
    // {
    //     var exist = await _db.Queryable<DbDevice>().Where(d => d.Name == device.Name).FirstAsync();
    //     if (exist != null) 
    //         throw new InvalidOperationException("设备名称已经存在。");
    //     var dbDevice = new DbDevice();
    //     device.CopyTo(dbDevice);
    //     dbDevice.VariableTables = new List<DbVariableTable>();
    //     // 添加默认变量表
    //     var dbVariableTable = new DbVariableTable();
    //     dbVariableTable.Name = "默认变量表";
    //     dbVariableTable.Description = "默认变量表";
    //     dbVariableTable.ProtocolType = dbDevice.ProtocolType;
    //     dbDevice.VariableTables.Add(dbVariableTable);
    //     var addDbDevice= await _db.InsertNav(dbDevice).Include(d => d.VariableTables).ExecuteReturnEntityAsync();
    //     return addDbDevice.CopyTo<Device>();
    // }

    public async Task<List<VariableTable>> GetAll()
    {
        var dbVariableTables = await _db.Queryable<DbVariableTable>().Includes(dv=>dv.Device).ToListAsync();
        var variableTables = new List<VariableTable>();
        
        foreach (var dbVariableTable in dbVariableTables)
            variableTables.Add(dbVariableTable.CopyTo<VariableTable>());
        
        return variableTables;
    }

    public async Task<DbVariableTable> GetById(int id)
    {
        return await _db.Queryable<DbVariableTable>().FirstAsync(p => p.Id == id);
    }

    public async Task<int> DeleteById(int id)
    {
        return await _db.Deleteable<DbVariableTable>(new DbVariableTable { Id = id }).ExecuteCommandAsync();
    }
}