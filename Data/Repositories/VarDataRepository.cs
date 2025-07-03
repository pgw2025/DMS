using System.Collections.Generic;
using System.Threading.Tasks;
using PMSWPF.Data.Entities;
using PMSWPF.Extensions;
using PMSWPF.Models;

namespace PMSWPF.Data.Repositories;

/// <summary>
/// VariableData仓储类，用于操作DbVariableData实体
/// </summary>
public class VarDataRepository
{
    /// <summary>
    /// 根据ID获取VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<DbVariableData> GetByIdAsync(int id)
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Queryable<DbVariableData>().In(id).SingleAsync();
        }
    }

    /// <summary>
    /// 获取所有VariableData
    /// </summary>
    /// <returns></returns>
    public async Task<List<VariableData>> GetAllAsync()
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Queryable<DbVariableData>().Select(dbVarData => dbVarData.CopyTo<VariableData>())
                .ToListAsync();
        }
    }

    /// <summary>
    /// 新增VariableData
    /// </summary>
    /// <param name="variableData">VariableData实体</param>
    /// <returns></returns>
    public async Task<VariableData> AddAsync(VariableData variableData)
    {
        using (var _db = DbContext.GetInstance())
        {
            var dbVarData = await _db.Insertable(variableData.CopyTo<DbVariableData>()).ExecuteReturnEntityAsync();
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
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Updateable(variableData.CopyTo<DbVariableData>()).ExecuteCommandAsync();
        }
    }

    /// <summary>
    /// 根据ID删除VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(int id)
    {
        using (var _db = DbContext.GetInstance())
        {
            return await _db.Deleteable<DbVariableData>().In(id).ExecuteCommandAsync();
        }
    }
}