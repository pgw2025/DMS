using AutoMapper;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services
{
    public class VarDataService
    {
        private readonly IMapper _mapper;
        private readonly SqlSugarDbContext _dbContext;
        private SqlSugarClient Db => _dbContext.GetInstance();

        public VarDataService(IMapper mapper, SqlSugarDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
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
    }
}
