using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using PMSWPF.Data.Entities;
using PMSWPF.Extensions;
using PMSWPF.Models;
using SqlSugar;

namespace PMSWPF.Data.Repositories;

/// <summary>
/// VariableData仓储类，用于操作DbVariableData实体
/// </summary>
public class VarDataRepository
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 根据ID获取VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<DbVariableData> GetByIdAsync(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Queryable<DbVariableData>()
                                  .In(id)
                                  .SingleAsync();
            stopwatch.Stop();
            Logger.Info($"根据ID '{id}' 获取VariableData耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 获取所有VariableData
    /// </summary>
    /// <returns></returns>
    public async Task<List<VariableData>> GetAllAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Queryable<DbVariableData>()
                                  .ToListAsync();
            stopwatch.Stop();
            Logger.Info($"获取所有VariableData耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result.Select(d=>d.CopyTo<VariableData>()).ToList();
        }
    }

    /// <summary>
    /// 获取所有VariableData
    /// </summary>
    /// <returns></returns>
    public async Task<List<VariableData>> GetByVariableTableId(int varTableId)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Queryable<DbVariableData>()
                                  .Where(d => d.VariableTableId == varTableId)
                                  .Select(dbVarData => dbVarData.CopyTo<VariableData>())
                                  .ToListAsync();
            stopwatch.Stop();
            Logger.Info($"获取变量表的所有变量{result.Count()}个耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 新增VariableData
    /// </summary>
    /// <param name="variableData">VariableData实体</param>
    /// <returns></returns>
    public async Task<VariableData> AddAsync(VariableData variableData)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var varData = await AddAsync(variableData, db);
            stopwatch.Stop();
            Logger.Info($"新增VariableData '{variableData.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return varData;
        }
    }


    /// <summary>
    /// 新增VariableData
    /// </summary>
    /// <param name="variableData">VariableData实体</param>
    /// <returns></returns>
    public async Task<VariableData> AddAsync(VariableData variableData, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var dbVarData = await db.Insertable(variableData.CopyTo<DbVariableData>())
                                .ExecuteReturnEntityAsync();
        stopwatch.Stop();
        Logger.Info($"新增VariableData '{variableData.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return dbVarData.CopyTo<VariableData>();
    }

    /// <summary>
    /// 新增VariableData
    /// </summary>
    /// <param name="variableData">VariableData实体</param>
    /// <returns></returns>
    public async Task<int> AddAsync(IEnumerable<VariableData> variableDatas)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var varData = await AddAsync(variableDatas, db);
            stopwatch.Stop();
            Logger.Info($"新增VariableData '{variableDatas.Count()}'个， 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return varData;
        }
    }


    /// <summary>
    /// 新增VariableData
    /// </summary>
    /// <param name="variableData">VariableData实体</param>
    /// <returns></returns>
    public async Task<int> AddAsync(IEnumerable<VariableData> variableDatas, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Stopwatch stopwatch2 = new Stopwatch();
        stopwatch2.Start();
        var dbList = variableDatas.Select(vb => vb.CopyTo<DbVariableData>()).ToList();
        stopwatch2.Stop();
        Logger.Info($"复制 VariableData'{variableDatas.Count()}'个， 耗时：{stopwatch2.ElapsedMilliseconds}ms");

        var res = await db.Insertable<DbVariableData>(dbList)
                          .ExecuteCommandAsync();

        stopwatch.Stop();
        Logger.Info($"新增VariableData '{variableDatas.Count()}'个， 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return res;
    }


    /// <summary>
    /// 更新VariableData
    /// </summary>
    /// <param name="variableData">VariableData实体</param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(VariableData variableData)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Updateable(variableData.CopyTo<DbVariableData>())
                                  .ExecuteCommandAsync();
            stopwatch.Stop();
            Logger.Info($"更新VariableData '{variableData.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 更新VariableData
    /// </summary>
    /// <param name="variableData">VariableData实体</param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(List<VariableData> variableDatas)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await UpdateAsync(variableDatas, _db);

            stopwatch.Stop();
            Logger.Info($"更新VariableData  {variableDatas.Count()}个 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 更新VariableData
    /// </summary>
    /// <param name="variableData">VariableData实体</param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(List<VariableData> variableDatas, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var dbVarDatas = variableDatas.Select(vd => vd.CopyTo<DbVariableData>());
        var result = await db.Updateable(dbVarDatas.ToList())
                             .ExecuteCommandAsync();

        stopwatch.Stop();
        Logger.Info($"更新VariableData  {variableDatas.Count()}个 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 删除VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(VariableData variableData)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using var db = DbContext.GetInstance();
        var result = await DeleteAsync(variableData, db);
        stopwatch.Stop();
        Logger.Info($"删除VariableData: '{variableData.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 根据ID删除VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(VariableData variableData, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using var _db = DbContext.GetInstance();

        var result = await _db.Deleteable<DbVariableData>()
                              .Where(d => d.Id == variableData.Id)
                              .ExecuteCommandAsync();
        stopwatch.Stop();
        Logger.Info($"删除VariableData: '{variableData.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 删除VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(IEnumerable<VariableData> variableDatas)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using var db = DbContext.GetInstance();

        var result = await DeleteAsync(variableDatas, db);

        stopwatch.Stop();
        Logger.Info($"删除VariableData: '{variableDatas.Count()}'个 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 根据ID删除VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(IEnumerable<VariableData> variableDatas, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using var _db = DbContext.GetInstance();

        var dbList = variableDatas.Select(vd => vd.CopyTo<DbVariableData>());
        var result = await _db.Deleteable<DbVariableData>(dbList)
                              .ExecuteCommandAsync();
        stopwatch.Stop();
        Logger.Info($"删除VariableData: '{variableDatas.Count()}'个 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
}