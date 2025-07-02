using iNKORE.UI.WPF.Modern.Common.IconKeys;
using PMSWPF.Data.Entities;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Models;
using SqlSugar;

namespace PMSWPF.Data.Repositories;

public class VarTableRepository
{
    /// <summary>
    /// 添加变量表
    /// </summary>
    /// <param name="varTable"></param>
    /// <returns>变量表的ID</returns>
    public async Task<int> Add(VariableTable varTable)
    {
        using (var db = DbContext.GetInstance())
        {
            return await db.Insertable<DbVariableTable>(varTable.CopyTo<DbVariableTable>())
                .ExecuteReturnIdentityAsync();
        }
    }

    
    /// <summary>
    /// 添加默认变量表
    /// </summary>
    /// <param name="db"></param>
    /// <param name="dbDevice"></param>
    /// <returns></returns>
    public async Task<DbVariableTable> AddDeviceDefVarTable(DbDevice dbDevice, SqlSugarClient db)
    {
        
        // 添加默认变量表
        dbDevice.VariableTables = new List<DbVariableTable>();
        var dbVariableTable = new DbVariableTable();
        dbVariableTable.IsActive = true;
        dbVariableTable.DeviceId=dbDevice.Id;
        dbVariableTable.Name = "默认变量表";
        dbVariableTable.Description = "默认变量表";
        dbVariableTable.ProtocolType = dbDevice.ProtocolType;
        dbDevice.VariableTables.Add(dbVariableTable);
        return await db.Insertable<DbVariableTable>(dbVariableTable).ExecuteReturnEntityAsync();;
    }


    public async Task<int> Edit(VariableTable variableTable)
    {
        using (var db = DbContext.GetInstance())
        {
            return await db.Updateable<DbVariableTable>(variableTable.CopyTo<DbVariableTable>()).ExecuteCommandAsync();
        }
    }
}