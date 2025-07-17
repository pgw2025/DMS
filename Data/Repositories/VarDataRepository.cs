using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using PMSWPF.Data.Entities;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Models;
using SqlSugar;
using AutoMapper;

namespace PMSWPF.Data.Repositories;

/// <summary>
/// VariableData仓储类，用于操作DbVariableData实体
/// </summary>
public class VarDataRepository
{
    private readonly IMapper _mapper;

    public VarDataRepository(IMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 根据ID获取VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <returns></returns>
    public async Task<DbVariableData> GetByIdAsync(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await GetByIdAsync(id, db);
            stopwatch.Stop();
            NlogHelper.Info($"根据ID '{id}' 获取VariableData耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 根据ID获取VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <param name="db">SqlSugarClient实例</param>
    /// <returns></returns>
    public async Task<DbVariableData> GetByIdAsync(int id, SqlSugarClient db)
    {
        return await db.Queryable<DbVariableData>()
                       .In(id)
                       .SingleAsync();
    }

    /// <summary>
    /// 获取所有VariableData
    /// </summary>
    /// <returns></returns>
    public async Task<List<VariableData>> GetAllAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await GetAllAsync(db);
            stopwatch.Stop();
            NlogHelper.Info($"获取所有VariableData耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 获取所有VariableData
    /// </summary>
    /// <param name="db">SqlSugarClient实例</param>
    /// <returns></returns>
    public async Task<List<VariableData>> GetAllAsync(SqlSugarClient db)
    {
        var result = await db.Queryable<DbVariableData>()
                             .Includes(d => d.VariableTable)
                             .Includes(d => d.VariableTable.Device)
                             .ToListAsync();
        return result.Select(d => _mapper.Map<VariableData>(d))
                     .ToList();
    }

    /// <summary>
    /// 获取所有VariableData
    /// </summary>
    /// <returns></returns>
    public async Task<List<VariableData>> GetByVariableTableIdAsync(int varTableId)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await GetByVariableTableIdAsync(varTableId, db);
            stopwatch.Stop();
            NlogHelper.Info($"获取变量表的所有变量{result.Count()}个耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 获取所有VariableData
    /// </summary>
    /// <param name="db">SqlSugarClient实例</param>
    /// <returns></returns>
    public async Task<List<VariableData>> GetByVariableTableIdAsync(int varTableId, SqlSugarClient db)
    {
        var result = await db.Queryable<DbVariableData>()
                             .Where(d => d.VariableTableId == varTableId)
                             .ToListAsync();
        return result.Select(d => _mapper.Map<VariableData>(d))
                     .ToList();
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
            NlogHelper.Info($"新增VariableData '{variableData.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
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
        var dbVarData = await db.Insertable(_mapper.Map<DbVariableData>(variableData))
                                .ExecuteReturnEntityAsync();
        stopwatch.Stop();
        NlogHelper.Info($"新增VariableData '{variableData.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return _mapper.Map<VariableData>(dbVarData);
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
            NlogHelper.Info($"新增VariableData '{variableDatas.Count()}'个， 耗时：{stopwatch.ElapsedMilliseconds}ms");
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
        var dbList = variableDatas.Select(vb => _mapper.Map<DbVariableData>(vb))
                                  .ToList();
        stopwatch2.Stop();
        NlogHelper.Info($"复制 VariableData'{variableDatas.Count()}'个， 耗时：{stopwatch2.ElapsedMilliseconds}ms");

        var res = await db.Insertable<DbVariableData>(dbList)
                          .ExecuteCommandAsync();

        stopwatch.Stop();
        NlogHelper.Info($"新增VariableData '{variableDatas.Count()}'个， 耗时：{stopwatch.ElapsedMilliseconds}ms");
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
        using (var db = DbContext.GetInstance())
        {
            var result = await UpdateAsync(variableData, db);
            stopwatch.Stop();
            NlogHelper.Info($"更新VariableData '{variableData.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 更新VariableData
    /// </summary>
    /// <param name="variableData">VariableData实体</param>
    /// <param name="db">SqlSugarClient实例</param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(VariableData variableData, SqlSugarClient db)
    {
        var result = await db.Updateable<DbVariableData>(_mapper.Map<DbVariableData>(variableData))
                             .ExecuteCommandAsync();
        return result;
    }

    /// <summary>
    /// 更新VariableData
    /// </summary>
    /// <param name="variableData">VariableData实体</param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(List<VariableData> variableDatas)
    {
        using var _db = DbContext.GetInstance();
        return await UpdateAsync(variableDatas, _db);
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

        var dbVarDatas = variableDatas.Select(vd => _mapper.Map<DbVariableData>(vd));
        var result = await db.Updateable<DbVariableData>(dbVarDatas.ToList())
                             .ExecuteCommandAsync();

        stopwatch.Stop();
        NlogHelper.Info($"更新VariableData  {variableDatas.Count()}个 耗时：{stopwatch.ElapsedMilliseconds}ms");
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
        NlogHelper.Info($"删除VariableData: '{variableData.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 根据ID删除VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <param name="db">SqlSugarClient实例</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(VariableData variableData, SqlSugarClient db)
    {
        var result = await db.Deleteable<DbVariableData>()
                             .Where(d => d.Id == variableData.Id)
                             .ExecuteCommandAsync();
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
        NlogHelper.Info($"删除VariableData: '{variableDatas.Count()}'个 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 删除VariableData
    /// </summary>
    /// <param name="id">主键ID</param>
    /// <param name="db">SqlSugarClient实例</param>
    /// <returns></returns>
    public async Task<int> DeleteAsync(IEnumerable<VariableData> variableDatas, SqlSugarClient db)
    {
        var dbList = variableDatas.Select(vd => _mapper.Map<DbVariableData>(vd))
                                  .ToList();
        var result = await db.Deleteable<DbVariableData>(dbList)
                             .ExecuteCommandAsync();
        return result;
    }

    // public VarDataRepository(IMapper mapper)
    // {
    //     _mapper = mapper;
    // }

    /// <summary>
    /// 为变量添加MQTT服务器关联，并指定别名。
    /// </summary>
    /// <param name="variableMqttList"></param>
    /// <param name="variableDatas">要添加MQTT服务器的变量数据列表。</param>
    /// <returns>成功添加或更新关联的数量。</returns>
    public async Task<int> AddMqttToVariablesAsync(IEnumerable<VariableMqtt> variableMqttList)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        int affectedCount = 0;
        using var db = DbContext.GetInstance();
        //插入列表
        List<DbVariableMqtt> insertDbVM = new List<DbVariableMqtt>();

        foreach (var variableMqtt in variableMqttList)
        {
            var existingAlias = await db.Queryable<DbVariableMqtt>()
                                        .Where(it => it.VariableDataId == variableMqtt.VariableData.Id && it.MqttId == variableMqtt.Mqtt.Id)
                                        .FirstAsync();
            if (existingAlias == null)
            {
                // 如果不存在，则添加新的关联
                insertDbVM.Add(new DbVariableMqtt
                           {
                               VariableDataId = variableMqtt.VariableData.Id,
                               MqttId = variableMqtt.Mqtt.Id,
                               MqttAlias = variableMqtt.MqttAlias,
                               CreateTime = DateTime.Now,
                               UpdateTime = DateTime.Now
                           });
                        
                affectedCount++;
            }
            else if (existingAlias.MqttAlias !=variableMqtt.MqttAlias)
            {
                // 如果存在但别名不同，则更新别名
                await db.Updateable<DbVariableMqtt>()
                        .SetColumns(it => it.MqttAlias == variableMqtt.MqttAlias)
                        .Where(it => it.VariableDataId == variableMqtt.VariableData.Id && it.MqttId == variableMqtt.Mqtt.Id)
                        .ExecuteCommandAsync();
                NlogHelper.Info(
                    $"为 {variableMqtt.VariableData.Name} 变量更新MQTT服务器{variableMqtt.Mqtt.Name}关联.");
                affectedCount++;
            }
        }

        if (insertDbVM.Count > 0)
        {
            await db.Insertable<DbVariableMqtt>(insertDbVM).ExecuteCommandAsync();
            NlogHelper.Info(
                        $"为 {insertDbVM.Count} 个变量添加MQTT服务器  关联，成功影响 {affectedCount} 个，耗时：{stopwatch.ElapsedMilliseconds}ms");
        }

        stopwatch.Stop();
        
        return affectedCount;
    }
}