using System.Diagnostics;
using AutoMapper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using Microsoft.Extensions.Logging;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 变量仓储实现类，负责变量数据的持久化操作。
/// 继承自 <see cref="BaseRepository{DbVariable}"/> 并实现 <see cref="IVariableRepository"/> 接口。
/// </summary>
public class VariableRepository : BaseRepository<DbVariable>, IVariableRepository
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    /// <param name="logger">日志记录器实例。</param>
    public VariableRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<VariableRepository> logger)
        : base(dbContext, logger)
    {
        _mapper = mapper;
    }


    /*
    /// <summary>
    /// 为变量添加MQTT服务器关联，并指定别名。（此方法当前被注释，可能为待实现或废弃功能）
    /// </summary>
    /// <param name="variableMqttList"></param>
    /// <param name="variableDatas">要添加MQTT服务器的变量数据列表。</param>
    /// <returns>成功添加或更新关联的数量。</returns>
     public async Task<int> AddMqttToVariablesAsync(IEnumerable<VariableMqtt> variableMqttList)
    {
        await _dbContext.GetInstance().BeginTranAsync();

        try
        {
            int affectedCount = 0;
            var variableIds = variableMqttList.Select(vm => vm.Variable.Id).Distinct().ToList();
            var mqttIds = variableMqttList.Select(vm => vm.Mqtt.Id).Distinct().ToList();

            // 1. 一次性查询所有相关的现有别名
            var existingAliases = await _dbContext.GetInstance().Queryable<DbVariableMqtt>()
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
                var updateResult = await _dbContext.GetInstance().Updateable(toUpdate).ExecuteCommandAsync();
                affectedCount += updateResult;
            }

            // 3. 批量插入
            if (toInsert.Any())
            {
                var insertResult = await _dbContext.GetInstance().Insertable(toInsert).ExecuteCommandAsync();
                affectedCount += insertResult;
            }

            await _dbContext.GetInstance().CommitTranAsync();
            //_logger.LogInformation($"成功为 {variableMqttList.Count()} 个变量请求添加/更新了MQTT服务器关联，实际影响 {affectedCount} 个。");
            return affectedCount;
        }
        catch (Exception ex)
        {
            await _dbContext.GetInstance().RollbackTranAsync();
            //_logger.LogError(ex, $"为变量添加MQTT服务器关联时发生错误: {ex.Message}");
            // 根据需要，可以向上层抛出异常
            throw;
        }
    }
*/
    /// <summary>
    /// 异步根据ID获取单个变量。
    /// </summary>
    /// <param name="id">变量的唯一标识符。</param>
    /// <returns>对应的变量实体，如果不存在则为null。</returns>
    public async Task<Variable> GetByIdAsync(int id)
    {
        var dbVariable = await base.GetByIdAsync(id);
        return _mapper.Map<Variable>(dbVariable);
    }

    /// <summary>
    /// 异步获取所有变量。
    /// </summary>
    /// <returns>包含所有变量实体的列表。</returns>
    public async Task<List<Variable>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<Variable>>(dbList);
    }

    /// <summary>
    /// 异步添加新变量。
    /// </summary>
    /// <param name="entity">要添加的变量实体。</param>
    /// <returns>添加成功后的变量实体（包含数据库生成的ID等信息）。</returns>
    public async Task<Variable> AddAsync(Variable entity)
    {
        var dbVariable = await base.AddAsync(_mapper.Map<DbVariable>(entity));
        return _mapper.Map(dbVariable, entity);
    }

    /// <summary>
    /// 异步更新现有变量。
    /// </summary>
    /// <param name="entity">要更新的变量实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAsync(Variable entity) => await base.UpdateAsync(_mapper.Map<DbVariable>(entity));

    /// <summary>
    /// 异步删除变量。
    /// </summary>
    /// <param name="entity">要删除的变量实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(Variable entity) => await base.DeleteAsync(_mapper.Map<DbVariable>(entity));
    
    
    /// <summary>
    /// 异步根据ID删除变量。
    /// </summary>
    /// <param name="id">要删除变量的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Deleteable(new DbVariable() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Delete {typeof(DbVariable)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 异步根据ID列表批量删除变量。
    /// </summary>
    /// <param name="ids">要删除变量的唯一标识符列表。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdsAsync(List<int> ids)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await _dbContext.GetInstance().Deleteable<DbVariable>()
                             .In(ids)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        _logger.LogInformation($"Delete {typeof(DbVariable)},Count={ids.Count},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 异步获取指定数量的变量。
    /// </summary>
    /// <param name="number">要获取的变量数量。</param>
    /// <returns>包含指定数量变量实体的列表。</returns>
    public new async Task<List<Variable>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<Variable>>(dbList);

    }

    public async Task<List<Variable>> AddBatchAsync(List<Variable> entities)
    {
        var dbEntities = _mapper.Map<List<DbVariable>>(entities);
        var addedEntities = await base.AddBatchAsync(dbEntities);
        return _mapper.Map<List<Variable>>(addedEntities);
    }

    /// <summary>
    /// 异步根据OPC UA NodeId获取单个变量实体。
    /// </summary>
    /// <param name="opcUaNodeId">OPC UA NodeId。</param>
    /// <returns>找到的变量实体，如果不存在则返回null。</returns>
    public async Task<Variable?> GetByOpcUaNodeIdAsync(string opcUaNodeId)
    {
        var dbVariable = await _dbContext.GetInstance().Queryable<DbVariable>()
                                 .Where(v => v.OpcUaNodeId == opcUaNodeId)
                                 .FirstAsync();
        return dbVariable == null ? null : _mapper.Map<Variable>(dbVariable);
    }

    /// <summary>
    /// 异步根据OPC UA NodeId列表获取变量实体列表。
    /// </summary>
    /// <param name="opcUaNodeIds">OPC UA NodeId列表。</param>
    /// <returns>找到的变量实体列表。</returns>
    public async Task<List<Variable>> GetByOpcUaNodeIdsAsync(List<string> opcUaNodeIds)
    {
        var dbVariables = await _dbContext.GetInstance().Queryable<DbVariable>()
                                  .Where(v => opcUaNodeIds.Contains(v.OpcUaNodeId))
                                  .ToListAsync();
        return _mapper.Map<List<Variable>>(dbVariables);
    }
}