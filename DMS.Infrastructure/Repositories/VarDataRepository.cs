using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Interfaces;
using System.Diagnostics;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Data;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// VariableData仓储类，用于操作DbVariableData实体
/// </summary>
public class VarDataRepository : BaseRepository<DbVariable, Variable>
{
    public VarDataRepository(IMapper mapper, ITransaction transaction)
        : base(mapper, transaction)
    {
    }

    

    public override async Task<List<Variable>> GetAllAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Queryable<DbVariable>()
                             .Includes(d => d.VariableTable)
                             .Includes(d => d.VariableTable.Device)
                             .ToListAsync();
        stopwatch.Stop();
        //NlogHelper.Info($"获取所有VariableData耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result.Select(d => _mapper.Map<Variable>(d))
                     .ToList();
    }

    public async Task<List<Variable>> GetByVariableTableIdAsync(int varTableId)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Queryable<DbVariable>()
                             .Where(d => d.VariableTableId == varTableId)
                             .ToListAsync();
        stopwatch.Stop();
        //NlogHelper.Info($"获取变量表的所有变量{result.Count()}个耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result.Select(d => _mapper.Map<Variable>(d))
                     .ToList();
    }

    public override async Task<int> AddAsync(Variable variable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var dbVarData = await Db.Insertable(_mapper.Map<DbVariable>(variable))
                                .ExecuteReturnEntityAsync();
        stopwatch.Stop();
        //NlogHelper.Info($"新增VariableData '{variable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return dbVarData.Id;
    }

    public async Task<int> AddAsync(IEnumerable<Variable> variableDatas)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        Stopwatch stopwatch2 = new Stopwatch();
        stopwatch2.Start();
        var dbList = variableDatas.Select(vb => _mapper.Map<DbVariable>(vb))
                                  .ToList();
        stopwatch2.Stop();
        //NlogHelper.Info($"复制 Variable'{variableDatas.Count()}'个， 耗时：{stopwatch2.ElapsedMilliseconds}ms");

        var res = await Db.Insertable<DbVariable>(dbList)
                          .ExecuteCommandAsync();

        stopwatch.Stop();
        //NlogHelper.Info($"新增VariableData '{variableDatas.Count()}'个， 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return res;
    }


    public override async Task<int> UpdateAsync(Variable variable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Updateable<DbVariable>(_mapper.Map<DbVariable>(variable))
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        //NlogHelper.Info($"更新VariableData '{variable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public async Task<int> UpdateAsync(List<Variable> variableDatas)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var dbVarDatas = variableDatas.Select(vd => _mapper.Map<DbVariable>(vd));
        var result = await Db.Updateable<DbVariable>(dbVarDatas.ToList())
                             .ExecuteCommandAsync();

        stopwatch.Stop();
        //NlogHelper.Info($"更新VariableData  {variableDatas.Count()}个 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public override async Task<int> DeleteAsync(Variable variable)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable<DbVariable>()
                             .Where(d => d.Id == variable.Id)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        //NlogHelper.Info($"删除VariableData: '{variable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public async Task<int> DeleteAsync(IEnumerable<Variable> variableDatas)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var dbList = variableDatas.Select(vd => _mapper.Map<DbVariable>(vd))
                                  .ToList();
        var result = await Db.Deleteable<DbVariable>(dbList)
                             .ExecuteCommandAsync();

        stopwatch.Stop();
        //NlogHelper.Info($"删除VariableData: '{variableDatas.Count()}'个 耗时：{stopwatch.ElapsedMilliseconds}ms");
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
        await Db.BeginTranAsync();

        try
        {
            int affectedCount = 0;
            var variableIds = variableMqttList.Select(vm => vm.Variable.Id).Distinct().ToList();
            var mqttIds = variableMqttList.Select(vm => vm.Mqtt.Id).Distinct().ToList();

            // 1. 一次性查询所有相关的现有别名
            var existingAliases = await Db.Queryable<DbVariableMqtt>()
                                          .Where(it => variableIds.Contains(it.VariableId) && mqttIds.Contains(it.MqttId))
                                          .ToListAsync();

            var existingAliasesDict = existingAliases
                .ToDictionary(a => (a.VariableId, a.MqttId), a => a);

            var toInsert = new List<DbVariableMqtt>();
            var toUpdate = new List<DbVariableMqtt>();

            foreach (var variableMqtt in variableMqttList)
            {
                var key = (variableMqtt.Variable.Id, variableMqtt.Mqtt.Id);
                if (existingAliasesDict.TryGetValue(key, out var existingAlias))
                {
                    // 如果存在但别名不同，则准备更新
                    // if (existingAlias.MqttAlias != variableMqtt.MqttAlias)
                    // {
                    //     existingAlias.MqttAlias = variableMqtt.MqttAlias;
                    //     existingAlias.UpdateTime = DateTime.Now;
                    //     toUpdate.Add(existingAlias);
                    // }
                }
                else
                {
                    // 如果不存在，则准备插入
                    toInsert.Add(new DbVariableMqtt
                    {
                        VariableId = variableMqtt.Variable.Id,
                        MqttId = variableMqtt.Mqtt.Id,
                        // MqttAlias = variableMqtt.MqttAlias,
                        CreateTime = DateTime.Now,
                        UpdateTime = DateTime.Now
                    });
                }
            }

            // 2. 批量更新
            if (toUpdate.Any())
            {
                var updateResult = await Db.Updateable(toUpdate).ExecuteCommandAsync();
                affectedCount += updateResult;
            }

            // 3. 批量插入
            if (toInsert.Any())
            {
                var insertResult = await Db.Insertable(toInsert).ExecuteCommandAsync();
                affectedCount += insertResult;
            }

            await Db.CommitTranAsync();
            //NlogHelper.Info($"成功为 {variableMqttList.Count()} 个变量请求添加/更新了MQTT服务器关联，实际影响 {affectedCount} 个。");
            return affectedCount;
        }
        catch (Exception ex)
        {
            await Db.RollbackTranAsync();
            //NlogHelper.Error($"为变量添加MQTT服务器关联时发生错误: {ex.Message}", ex);
            // 根据需要，可以向上层抛出异常
            throw;
        }
    }
}