using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Interfaces;
using DMS.Infrastructure.Entities;
using System.Diagnostics;
using DMS.Infrastructure.Data;

namespace DMS.Infrastructure.Repositories;

public class VarTableRepository : BaseRepository<DbVariableTable>
{
    public VarTableRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
    }

    /// <summary>
    /// 添加变量表
    /// </summary>
    /// <param name="varTable"></param>
    /// <returns>变量表的ID</returns>
    public override async Task<DbVariableTable> AddAsync(DbVariableTable entity)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var addVarTabel = await Db.Insertable(entity)
                                  .ExecuteReturnEntityAsync();

        stopwatch.Stop();
        //NlogHelper.Info($"添加变量表 '{entity.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return addVarTabel;
    }

    /// <summary>
    /// 编辑变量表
    /// </summary>
    /// <param name="variableTable"></param>
    /// <returns></returns>
    public override async Task<int> UpdateAsync(DbVariableTable entity)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Updateable(entity)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        //NlogHelper.Info($"编辑变量表 '{entity.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 删除变量表
    /// </summary>
    /// <param name="variableTable"></param>
    /// <returns></returns>
    public override async Task<int> DeleteAsync(DbVariableTable entity)
    {
        if (entity == null )
            return 0;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        // 转换对象
       var res= await Db.Deleteable(entity)
                .ExecuteCommandAsync();
       stopwatch.Stop();
       //NlogHelper.Info($"删除变量表 '{entity.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
       return res;
    }

    /// <summary>
    /// 删除变量表支持事务
    /// </summary>
    /// <param name="deviceVariableTables"></param>
    /// <param name="db"></param>
    public async Task DeleteAsync(IEnumerable<DbVariableTable> deviceVariableTables)
    {
        if (deviceVariableTables == null || deviceVariableTables.Count() == 0)
            return;
        // 转换对象
        await Db.Deleteable<DbVariableTable>(deviceVariableTables)
                .ExecuteCommandAsync();
    }



}