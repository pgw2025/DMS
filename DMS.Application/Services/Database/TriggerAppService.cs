using AutoMapper;
using DMS.Application.Interfaces.Database;
using DMS.Core.Interfaces;
using DMS.Core.Models.Triggers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using DMS.Application.Interfaces;

namespace DMS.Application.Services.Database
{
    /// <summary>
    /// 触发器应用服务，负责处理触发器相关的业务逻辑。
    /// 实现 <see cref="ITriggerAppService"/> 接口。
    /// </summary>
    public class TriggerAppService : ITriggerAppService
    {
        private readonly IRepositoryManager _repositoryManager;

        /// <summary>
        /// 构造函数，通过依赖注入获取仓储管理器和AutoMapper实例。
        /// </summary>
        /// <param name="repoManager">仓储管理器实例。</param>
        /// <param name="mapper">AutoMapper 实例。</param>
        public TriggerAppService(IRepositoryManager repositoryManager)
        {
            _repositoryManager = repositoryManager;
        }

        /// <summary>
        /// 异步根据ID获取触发器定义。
        /// </summary>
        /// <param name="id">触发器ID。</param>
        /// <returns>触发器定义实体。</returns>
        public async Task<Trigger> GetTriggerByIdAsync(int id)
        {
            var dbTrigger = await _repositoryManager.Triggers.GetByIdAsync(id);
            return dbTrigger;
        }

        /// <summary>
        /// 异步获取所有触发器定义。
        /// </summary>
        /// <returns>触发器定义实体列表。</returns>
        public async Task<List<Trigger>> GetAllTriggersAsync()
        {
            var triggers = await _repositoryManager.Triggers.GetAllAsync();
            var triggerVariables = await _repositoryManager.TriggerVariables.GetAllAsync();
            foreach (var trigger in triggers)
            {
                // 加载关联的变量ID
                var variables = triggerVariables.Where(t=>t.TriggerDefinitionId==trigger.Id).ToList();
                // trigger.Variables = variables;
            }

            return triggers;
        }

        /// <summary>
        /// 异步创建一个新触发器定义及其关联的变量ID。
        /// </summary>
        /// <param name="trigger">要创建的触发器定义。</param>
        /// <returns>新创建的触发器定义。</returns>
        public async Task<Trigger> CreateTriggerAsync(Trigger trigger)
        {
            try
            {
                await _repositoryManager.BeginTranAsync();

                // 添加触发器定义
                var addedTrigger = await _repositoryManager.Triggers.AddAsync(trigger);

                // // 添加关联的变量ID
                // if (trigger.Variables != null && trigger.Variables.Any())
                // {
                //     var triggerVariables = trigger.Variables.Select(variableId => new DbTriggerVariable
                //                                                           {
                //                                                               TriggerDefinitionId = addedTrigger.Id,
                //                                                               VariableId = variableId
                //                                                           })
                //                                   .ToList();
                //
                //     await _repositoryManager.AddTriggerVariablesAsync(triggerVariables);
                // }

                await _repositoryManager.CommitAsync();
                return addedTrigger;
            }
            catch
            {
                await _repositoryManager.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 异步更新一个已存在的触发器定义及其关联的变量ID。
        /// </summary>
        /// <param name="trigger">要更新的触发器定义。</param>
        /// <returns>受影响的行数。</returns>
        public async Task<int> UpdateTriggerAsync(Trigger trigger)
        {
            try
            {
                await _repositoryManager.BeginTranAsync();

                // 更新触发器定义
                var rowsAffected = await _repositoryManager.Triggers.UpdateAsync(trigger);

                // if (rowsAffected > 0)
                // {
                //     // 删除旧的关联关系
                //     await _repositoryManager.DeleteTriggerVariablesByTriggerIdAsync(trigger.Id);
                //
                //     // 插入新的关联关系
                //     if (trigger.Variables != null && trigger.Variables.Any())
                //     {
                //         var triggerVariables = trigger.Variables.Select(variableId => new DbTriggerVariable
                //                                                               {
                //                                                                   TriggerDefinitionId = trigger.Id,
                //                                                                   VariableId = variableId
                //                                                               })
                //                                       .ToList();
                //
                //         await _repositoryManager.AddTriggerVariablesAsync(triggerVariables);
                //     }
                // }

                await _repositoryManager.CommitAsync();
                return rowsAffected;
            }
            catch
            {
                await _repositoryManager.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 异步删除一个触发器定义及其关联的变量关系。
        /// </summary>
        /// <param name="id">要删除的触发器ID。</param>
        /// <returns>如果删除成功则为 true，否则为 false。</returns>
        public async Task<bool> DeleteTriggerByIdAsync(int id)
        {
            try
            {
                await _repositoryManager.BeginTranAsync();

                // // 删除关联的变量关系
                // await _repositoryManager.DeleteTriggerVariablesByTriggerIdAsync(id);

                // 删除触发器本身
                var rowsAffected = await _repositoryManager.Triggers.DeleteByIdAsync(id);

                await _repositoryManager.CommitAsync();
                return rowsAffected > 0;
            }
            catch
            {
                await _repositoryManager.RollbackAsync();
                throw;
            }
        }

        /// <summary>
        /// 异步获取指定变量ID关联的所有触发器定义。
        /// </summary>
        /// <param name="variableId">变量ID。</param>
        /// <returns>与指定变量关联的触发器定义实体列表。</returns>
        public async Task<IEnumerable<Trigger>> GetTriggersByVariableIdAsync(int variableId)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            //
            // // 获取关联的触发器ID列表
            // var triggerIds = await _repositoryManager.GetTriggerIdsByVariableIdAsync(variableId);

            // var triggers = new List<TriggerMenu>();
            // if (triggerIds.Any())
            // {
            //     // 获取所有关联的触发器
            //     foreach (var triggerId in triggerIds)
            //     {
            //         var trigger = await GetTriggerByIdAsync(triggerId);
            //         if (trigger != null)
            //         {
            //             triggers.Add(trigger);
            //         }
            //     }
            // }

            stopwatch.Stop();
            // 可选：记录日志
            // _logger.LogInformation($"GetTriggersByVariableId for VariableId={variableId},耗时：{stopwatch.ElapsedMilliseconds}ms");

            return null;
        }
    }
}