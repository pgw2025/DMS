using iNKORE.UI.WPF.Modern.Common.IconKeys;
using PMSWPF.Data.Entities;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Models;
using SqlSugar;
using System.Diagnostics;
using NLog;

namespace PMSWPF.Data.Repositories;

public class VarTableRepository
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 添加变量表
    /// </summary>
    /// <param name="varTable"></param>
    /// <returns>变量表的ID</returns>
    public async Task<int> Add(VariableTable varTable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await db.Insertable<DbVariableTable>(varTable.CopyTo<DbVariableTable>())
                .ExecuteReturnIdentityAsync();
            stopwatch.Stop();
            Logger.Info($"添加变量表 '{varTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
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
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        // 添加默认变量表
        dbDevice.VariableTables = new List<DbVariableTable>();
        var dbVariableTable = new DbVariableTable();
        dbVariableTable.IsActive = true;
        dbVariableTable.DeviceId=dbDevice.Id;
        dbVariableTable.Name = "默认变量表";
        dbVariableTable.Description = "默认变量表";
        dbVariableTable.ProtocolType = dbDevice.ProtocolType;
        dbDevice.VariableTables.Add(dbVariableTable);
        var result = await db.Insertable<DbVariableTable>(dbVariableTable).ExecuteReturnEntityAsync();
        stopwatch.Stop();
        Logger.Info($"添加设备 '{dbDevice.Name}' 的默认变量表耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }


    public async Task<int> Edit(VariableTable variableTable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await db.Updateable<DbVariableTable>(variableTable.CopyTo<DbVariableTable>()).ExecuteCommandAsync();
            stopwatch.Stop();
            Logger.Info($"编辑变量表 '{variableTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
}