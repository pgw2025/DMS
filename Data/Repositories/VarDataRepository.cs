using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NLog;
using PMSWPF.Data.Entities;
using PMSWPF.Extensions;
using PMSWPF.Models;

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
            var result = await _db.Queryable<DbVariableData>().In(id).SingleAsync();
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
            var result = await _db.Queryable<DbVariableData>().Select(dbVarData => dbVarData.CopyTo<VariableData>())
                .ToListAsync();
            stopwatch.Stop();
            Logger.Info($"获取所有VariableData耗时：{stopwatch.ElapsedMilliseconds}ms");
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
        using (var _db = DbContext.GetInstance())
        {
            var dbVarData = await _db.Insertable(variableData.CopyTo<DbVariableData>()).ExecuteReturnEntityAsync();
            stopwatch.Stop();
            Logger.Info($"新增VariableData '{variableData.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return dbVarData.CopyTo<VariableData>();
        }
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
            var result = await _db.Updateable(variableData.CopyTo<DbVariableData>()).ExecuteCommandAsync();
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
            var dbVarDatas = variableDatas.Select(vd=>vd.CopyTo<DbVariableData>());
            var result = await _db.Updateable(dbVarDatas.ToList()).ExecuteCommandAsync();
           
            stopwatch.Stop();
            Logger.Info($"更新VariableData  {variableDatas.Count()}个 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 根据ID删除VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var _db = DbContext.GetInstance())
        {
            var result = await _db.Deleteable<DbVariableData>().In(id).ExecuteCommandAsync();
            stopwatch.Stop();
            Logger.Info($"删除VariableData ID '{id}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
}