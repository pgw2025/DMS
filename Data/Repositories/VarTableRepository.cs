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
    public async Task<VariableTable> Add(VariableTable varTable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var addVarTable = await Add(varTable, db);

            stopwatch.Stop();
            Logger.Info($"添加变量表 '{varTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return addVarTable;
        }
    }


    /// <summary>
    /// 添加变量表支持事务
    /// </summary>
    /// <param name="db"></param>
    /// <param name="dbDevice"></param>
    /// <returns></returns>
    public async Task<VariableTable> Add(VariableTable variableTable, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var addVarTabel = await db.Insertable<DbVariableTable>(variableTable.CopyTo<DbVariableTable>())
                                  .ExecuteReturnEntityAsync();
        stopwatch.Stop();
        Logger.Info($"添加设备 '{addVarTabel.Name}' 的默认变量表耗时：{stopwatch.ElapsedMilliseconds}ms");
        return addVarTabel.CopyTo<VariableTable>();
    }

    /// <summary>
    /// 编辑变量表
    /// </summary>
    /// <param name="variableTable"></param>
    /// <returns></returns>
    public async Task<int> Edit(VariableTable variableTable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await Edit(variableTable, db);
            stopwatch.Stop();
            Logger.Info($"编辑变量表 '{variableTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 编辑变量表，支持事务
    /// </summary>
    /// <param name="variableTable"></param>
    /// <returns></returns>
    public async Task<int> Edit(VariableTable variableTable, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await db.Updateable<DbVariableTable>(variableTable.CopyTo<DbVariableTable>())
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        Logger.Info($"编辑变量表 '{variableTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 删除变量表
    /// </summary>
    /// <param name="variableTable"></param>
    /// <returns></returns>
    public async Task<int> Delete(VariableTable variableTable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await Delete(variableTable, db);
            stopwatch.Stop();
            Logger.Info($"删除变量表 '{variableTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
    
    /// <summary>
    /// 删除变量表支持事务
    /// </summary>
    /// <param name="deviceVariableTables"></param>
    /// <param name="db"></param>
    public async Task<int> Delete(VariableTable varTable, SqlSugarClient db)
    {
        if (varTable == null )
            return 0;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        // 转换对象
       var res= await db.Deleteable<DbVariableTable>(varTable.CopyTo<DbVariableTable>())
                .ExecuteCommandAsync();
       stopwatch.Stop();
       Logger.Info($"删除变量表 '{varTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
       return res;
    }

    /// <summary>
    /// 删除变量表支持事务
    /// </summary>
    /// <param name="deviceVariableTables"></param>
    /// <param name="db"></param>
    public async Task Delete(List<VariableTable> deviceVariableTables, SqlSugarClient db)
    {
        if (deviceVariableTables == null || deviceVariableTables.Count == 0)
            return;
        // 转换对象
        var dbList = deviceVariableTables.Select(v => v.CopyTo<DbVariableTable>())
                                         .ToList();
        await db.Deleteable<DbVariableTable>(dbList)
                .ExecuteCommandAsync();
    }
}