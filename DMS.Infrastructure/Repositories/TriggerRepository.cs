using System.Diagnostics;
using AutoMapper;
using DMS.Core.Interfaces.Repositories.Triggers;
using DMS.Core.Models.Triggers;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using Microsoft.Extensions.Logging;

namespace DMS.Infrastructure.Repositories
{
    /// <summary>
    /// 触发器仓储实现类，负责触发器数据的持久化操作。
    /// 继承自 <see cref="BaseRepository{DbTriggerDefinition}"/> 并实现 <see cref="ITriggerRepository"/> 接口。
    /// </summary>
    public class TriggerRepository : BaseRepository<DbTriggerDefinition>, ITriggerRepository
    {
        private readonly IMapper _mapper;

        /// <summary>
        /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
        /// </summary>
        /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
        /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
        /// <param name="logger">日志记录器实例。</param>
        public TriggerRepository(IMapper mapper, SqlSugarDbContext dbContext, ILogger<TriggerRepository> logger)
            : base(dbContext, logger)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// 获取所有触发器定义
        /// </summary>
        public async Task<IEnumerable<TriggerDefinition>> GetAllAsync()
        {
            var dbList = await base.GetAllAsync();
            // 加载关联的变量ID
            foreach (var dbTrigger in dbList)
            {
                var variableIds = await _dbContext.GetInstance()
                    .Queryable<DbTriggerVariable>()
                    .Where(tv => tv.TriggerDefinitionId == dbTrigger.Id)
                    .Select(tv => tv.VariableId)
                    .ToListAsync();
                dbTrigger.VariableIds = variableIds;
            }
            return _mapper.Map<List<TriggerDefinition>>(dbList);
        }

        /// <summary>
        /// 根据 ID 获取触发器定义
        /// </summary>
        public async Task<TriggerDefinition?> GetByIdAsync(int id)
        {
            var dbTrigger = await base.GetByIdAsync(id);
            if (dbTrigger != null)
            {
                // 加载关联的变量ID
                var variableIds = await _dbContext.GetInstance()
                    .Queryable<DbTriggerVariable>()
                    .Where(tv => tv.TriggerDefinitionId == dbTrigger.Id)
                    .Select(tv => tv.VariableId)
                    .ToListAsync();
                dbTrigger.VariableIds = variableIds;
            }
            return _mapper.Map<TriggerDefinition>(dbTrigger);
        }

        /// <summary>
        /// 添加一个新的触发器定义
        /// </summary>
        public async Task<TriggerDefinition> AddAsync(TriggerDefinition trigger)
        {
            var dbTrigger = _mapper.Map<DbTriggerDefinition>(trigger);
            dbTrigger = await base.AddAsync(dbTrigger);
            
            // 保存关联的变量ID
            if (trigger.VariableIds != null && trigger.VariableIds.Any())
            {
                var triggerVariables = trigger.VariableIds.Select(variableId => new DbTriggerVariable
                {
                    TriggerDefinitionId = dbTrigger.Id,
                    VariableId = variableId
                }).ToList();
                
                await _dbContext.GetInstance().Insertable(triggerVariables).ExecuteCommandAsync();
            }
            
            return _mapper.Map(dbTrigger, trigger);
        }

        /// <summary>
        /// 更新一个已存在的触发器定义
        /// </summary>
        public async Task<TriggerDefinition?> UpdateAsync(TriggerDefinition trigger)
        {
            var dbTrigger = _mapper.Map<DbTriggerDefinition>(trigger);
            var rowsAffected = await base.UpdateAsync(dbTrigger);
            
            if (rowsAffected > 0)
            {
                // 删除旧的关联关系
                await _dbContext.GetInstance()
                    .Deleteable<DbTriggerVariable>()
                    .Where(tv => tv.TriggerDefinitionId == dbTrigger.Id)
                    .ExecuteCommandAsync();
                
                // 插入新的关联关系
                if (trigger.VariableIds != null && trigger.VariableIds.Any())
                {
                    var triggerVariables = trigger.VariableIds.Select(variableId => new DbTriggerVariable
                    {
                        TriggerDefinitionId = dbTrigger.Id,
                        VariableId = variableId
                    }).ToList();
                    
                    await _dbContext.GetInstance().Insertable(triggerVariables).ExecuteCommandAsync();
                }
                
                return trigger;
            }
            
            return null;
        }

        /// <summary>
        /// 删除一个触发器定义
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            // 先删除关联的变量关系
            await _dbContext.GetInstance()
                .Deleteable<DbTriggerVariable>()
                .Where(tv => tv.TriggerDefinitionId == id)
                .ExecuteCommandAsync();
            
            // 再删除触发器本身
            var rowsAffected = await _dbContext.GetInstance().Deleteable<DbTriggerDefinition>()
                                       .In(id)
                                       .ExecuteCommandAsync();
            stopwatch.Stop();
            _logger.LogInformation($"Delete {typeof(DbTriggerDefinition).Name},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
            return rowsAffected > 0;
        }

        /// <summary>
        /// 获取与指定变量关联的所有触发器定义
        /// </summary>
        public async Task<IEnumerable<TriggerDefinition>> GetByVariableIdAsync(int variableId)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            
            // 先查询关联表获取触发器ID
            var triggerIds = await _dbContext.GetInstance()
                .Queryable<DbTriggerVariable>()
                .Where(tv => tv.VariableId == variableId)
                .Select(tv => tv.TriggerDefinitionId)
                .ToListAsync();
            
            // 再查询触发器定义
            var dbList = new List<DbTriggerDefinition>();
            if (triggerIds.Any())
            {
                dbList = await _dbContext.GetInstance().Queryable<DbTriggerDefinition>()
                                     .In(it => it.Id, triggerIds)
                                     .ToListAsync();
                
                // 加载每个触发器的变量ID列表
                foreach (var dbTrigger in dbList)
                {
                    var variableIds = await _dbContext.GetInstance()
                        .Queryable<DbTriggerVariable>()
                        .Where(tv => tv.TriggerDefinitionId == dbTrigger.Id)
                        .Select(tv => tv.VariableId)
                        .ToListAsync();
                    dbTrigger.VariableIds = variableIds;
                }
            }
            
            stopwatch.Stop();
            _logger.LogInformation($"GetByVariableId {typeof(DbTriggerDefinition).Name},VariableId={variableId},耗时：{stopwatch.ElapsedMilliseconds}ms");
            return _mapper.Map<List<TriggerDefinition>>(dbList);
        }
    }
}