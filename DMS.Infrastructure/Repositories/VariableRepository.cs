using System.Diagnostics;
using AutoMapper;
using DMS.Core.Helper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

/// <summary>
///     VariableData仓储类，用于操作DbVariableData实体
/// </summary>
public class VariableRepository : BaseRepository<DbVariable>, IVariableRepository
{
    private readonly IMapper _mapper;

    public VariableRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
    {
        _mapper = mapper;
    }


    /*
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
                .ToDictionary(a => (a.VariableId, a.Mqtt.Id), a => a);

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
*/
    public async Task<Variable> GetByIdAsync(int id)
    {
        var dbVariable = await base.GetByIdAsync(id);
        return _mapper.Map<Variable>(dbVariable);
    }

    public async Task<List<Variable>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<Variable>>(dbList);
    }

    public async Task<Variable> AddAsync(Variable entity)
    {
        var dbVariable = await base.AddAsync(_mapper.Map<DbVariable>(entity));
        return _mapper.Map(dbVariable, entity);
    }

    public async Task<int> UpdateAsync(Variable entity) => await base.UpdateAsync(_mapper.Map<DbVariable>(entity));

    public async Task<int> DeleteAsync(Variable entity) => await base.DeleteAsync(_mapper.Map<DbVariable>(entity));
    
    
    public async Task<int> DeleteAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new Variable() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbMenu)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    public new async Task<List<Variable>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<Variable>>(dbList);

    }
}