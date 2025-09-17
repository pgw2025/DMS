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
            return _mapper.Map<List<TriggerDefinition>>(dbList);
        }

        /// <summary>
        /// 根据 ID 获取触发器定义
        /// </summary>
        public async Task<TriggerDefinition?> GetByIdAsync(int id)
        {
            var dbTrigger = await base.GetByIdAsync(id);
            return _mapper.Map<TriggerDefinition>(dbTrigger);
        }

        /// <summary>
        /// 添加一个新的触发器定义
        /// </summary>
        public async Task<TriggerDefinition> AddAsync(TriggerDefinition trigger)
        {
            var dbTrigger = await base.AddAsync(_mapper.Map<DbTriggerDefinition>(trigger));
            return _mapper.Map(dbTrigger, trigger);
        }

        /// <summary>
        /// 更新一个已存在的触发器定义
        /// </summary>
        public async Task<TriggerDefinition?> UpdateAsync(TriggerDefinition trigger)
        {
            var rowsAffected = await base.UpdateAsync(_mapper.Map<DbTriggerDefinition>(trigger));
            return rowsAffected > 0 ? trigger : null;
        }

        /// <summary>
        /// 删除一个触发器定义
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
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
            var dbList = await _dbContext.GetInstance().Queryable<DbTriggerDefinition>()
                                 .Where(t => t.VariableId == variableId)
                                 .ToListAsync();
            stopwatch.Stop();
            _logger.LogInformation($"GetByVariableId {typeof(DbTriggerDefinition).Name},VariableId={variableId},耗时：{stopwatch.ElapsedMilliseconds}ms");
            return _mapper.Map<List<TriggerDefinition>>(dbList);
        }
    }
}