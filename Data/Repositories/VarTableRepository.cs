using iNKORE.UI.WPF.Modern.Common.IconKeys;
using PMSWPF.Data.Entities;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Models;
using SqlSugar;
using System.Diagnostics;
using AutoMapper;

namespace PMSWPF.Data.Repositories;

public class VarTableRepository
{
    private readonly IMapper _mapper;

    public VarTableRepository(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 添加变量表
    /// </summary>
    /// <param name="varTable"></param>
    /// <returns>变量表的ID</returns>
    public async Task<VariableTable> AddAsync(VariableTable varTable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var addVarTable = await AddAsync(varTable, db);

            stopwatch.Stop();
            NlogHelper.Info($"添加变量表 '{varTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return addVarTable;
        }
    }


    /// <summary>
    /// 添加变量表支持事务
    /// </summary>
    /// <param name="db"></param>
    /// <param name="dbDevice"></param>
    /// <returns></returns>
    public async Task<VariableTable> AddAsync(VariableTable variableTable, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var addVarTabel = await db.Insertable<DbVariableTable>(_mapper.Map<DbVariableTable>(variableTable))
                                  .ExecuteReturnEntityAsync();
        stopwatch.Stop();
        NlogHelper.Info($"添加设备 '{addVarTabel.Name}' 的默认变量表耗时：{stopwatch.ElapsedMilliseconds}ms");
        return _mapper.Map<VariableTable>(addVarTabel);
    }

    /// <summary>
    /// 编辑变量表
    /// </summary>
    /// <param name="variableTable"></param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(VariableTable variableTable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using var db = DbContext.GetInstance();
        var result = await UpdateAsync(variableTable, db);
        stopwatch.Stop();
        NlogHelper.Info($"编辑变量表 '{variableTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 编辑变量表，支持事务
    /// </summary>
    /// <param name="variableTable"></param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(VariableTable variableTable, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await db.Updateable<DbVariableTable>(_mapper.Map<DbVariableTable>(variableTable))
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"编辑变量表 '{variableTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 删除变量表
    /// </summary>
    /// <param name="variableTable"></param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(VariableTable variableTable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await DeleteAsync(variableTable, db);
            stopwatch.Stop();
            NlogHelper.Info($"删除变量表 '{variableTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
    
    /// <summary>
    /// 删除变量表支持事务
    /// </summary>
    /// <param name="deviceVariableTables"></param>
    /// <param name="db"></param>
    public async Task<int> DeleteAsync(VariableTable varTable, SqlSugarClient db)
    {
        if (varTable == null )
            return 0;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        // 转换对象
       var res= await db.Deleteable<DbVariableTable>(_mapper.Map<DbVariableTable>(varTable))
                .ExecuteCommandAsync();
       stopwatch.Stop();
       NlogHelper.Info($"删除变量表 '{varTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
       return res;
    }

    /// <summary>
    /// 删除变量表支持事务
    /// </summary>
    /// <param name="deviceVariableTables"></param>
    /// <param name="db"></param>
    public async Task DeleteAsync(IEnumerable<VariableTable> deviceVariableTables, SqlSugarClient db)
    {
        if (deviceVariableTables == null || deviceVariableTables.Count() == 0)
            return;
        // 转换对象
        var dbList = deviceVariableTables.Select(v => _mapper.Map<DbVariableTable>(v))
                                         .ToList();
        await db.Deleteable<DbVariableTable>(dbList)
                .ExecuteCommandAsync();
    }



}