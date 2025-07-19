using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Interfaces;
using DMS.Infrastructure.Entities;
using System.Diagnostics;
using DMS.Infrastructure.Data;

namespace DMS.Infrastructure.Repositories;

public class VarTableRepository : BaseRepository<DbVariableTable, VariableTable>
{
    public VarTableRepository(IMapper mapper, ITransaction transaction)
        : base(mapper, transaction)
    {
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
        var addVarTabel = await Db.Insertable<DbVariableTable>(_mapper.Map<DbVariableTable>(varTable))
                                  .ExecuteReturnEntityAsync();

        stopwatch.Stop();
        //NlogHelper.Info($"添加变量表 '{varTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
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
        var result = await Db.Updateable<DbVariableTable>(_mapper.Map<DbVariableTable>(variableTable))
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        //NlogHelper.Info($"编辑变量表 '{variableTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 删除变量表
    /// </summary>
    /// <param name="variableTable"></param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(VariableTable varTable)
    {
        if (varTable == null )
            return 0;
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        // 转换对象
       var res= await Db.Deleteable<DbVariableTable>(_mapper.Map<DbVariableTable>(varTable))
                .ExecuteCommandAsync();
       stopwatch.Stop();
       //NlogHelper.Info($"删除变量表 '{varTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
       return res;
    }

    /// <summary>
    /// 删除变量表支持事务
    /// </summary>
    /// <param name="deviceVariableTables"></param>
    /// <param name="db"></param>
    public async Task DeleteAsync(IEnumerable<VariableTable> deviceVariableTables)
    {
        if (deviceVariableTables == null || deviceVariableTables.Count() == 0)
            return;
        // 转换对象
        var dbList = deviceVariableTables.Select(v => _mapper.Map<DbVariableTable>(v))
                                         .ToList();
        await Db.Deleteable<DbVariableTable>(dbList)
                .ExecuteCommandAsync();
    }



}