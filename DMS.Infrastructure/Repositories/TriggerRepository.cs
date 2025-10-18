using System.Diagnostics;
using AutoMapper;
using DMS.Core.Interfaces.Repositories.Triggers;
using DMS.Core.Models.Triggers;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

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
        /// 异步根据ID获取单个触发器定义。
        /// </summary>
        /// <param name="id">触发器定义的唯一标识符。</param>
        /// <returns>对应的触发器定义实体，如果不存在则为null。</returns>
        public async Task<Trigger> GetByIdAsync(int id)
        {
            var dbTrigger = await base.GetByIdAsync(id);
            return _mapper.Map<Trigger>(dbTrigger);
        }

        /// <summary>
        /// 异步获取所有触发器定义。
        /// </summary>
        /// <returns>包含所有触发器定义实体的列表。</returns>
        public async Task<List<Trigger>> GetAllAsync()
        {
            var dbList = await base.GetAllAsync();
            return _mapper.Map<List<Trigger>>(dbList);
        }

        /// <summary>
        /// 异步添加新触发器定义。
        /// </summary>
        /// <param name="entity">要添加的触发器定义实体。</param>
        /// <returns>添加成功后的触发器定义实体（包含数据库生成的ID等信息）。</returns>
        public async Task<Trigger> AddAsync(Trigger entity)
        {
            var dbTrigger = _mapper.Map<DbTriggerDefinition>(entity);
            var addedDbTrigger = await base.AddAsync(dbTrigger);
            return _mapper.Map(addedDbTrigger, entity);
        }

        /// <summary>
        /// 异步更新现有触发器定义。
        /// </summary>
        /// <param name="entity">要更新的触发器定义实体。</param>
        /// <returns>受影响的行数。</returns>
        public async Task<int> UpdateAsync(Trigger entity)
        {
            var dbTrigger = _mapper.Map<DbTriggerDefinition>(entity);
            return await base.UpdateAsync(dbTrigger);
        }

        /// <summary>
        /// 异步删除触发器定义。
        /// </summary>
        /// <param name="entity">要删除的触发器定义实体。</param>
        /// <returns>受影响的行数。</returns>
        public async Task<int> DeleteAsync(Trigger entity)
        {
            return await base.DeleteAsync(_mapper.Map<DbTriggerDefinition>(entity));
        }

        /// <summary>
        /// 异步根据ID删除触发器定义。
        /// </summary>
        /// <param name="id">要删除触发器定义的唯一标识符。</param>
        /// <returns>受影响的行数。</returns>
        public async Task<int> DeleteByIdAsync(int id)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await _dbContext.GetInstance().Deleteable(new DbTriggerDefinition() { Id = id })
                                 .ExecuteCommandAsync();
            stopwatch.Stop();
            _logger.LogInformation($"Delete {typeof(DbTriggerDefinition)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
        
        /// <summary>
        /// 异步获取指定数量的触发器定义。
        /// </summary>
        /// <param name="number">要获取的触发器定义数量。</param>
        /// <returns>包含指定数量触发器定义实体的列表。</returns>
        public new async Task<List<Trigger>> TakeAsync(int number)
        {
            var dbList = await base.TakeAsync(number);
            return _mapper.Map<List<Trigger>>(dbList);
        }

        public async Task<List<Trigger>> AddBatchAsync(List<Trigger> entities)
        {
            var dbEntities = _mapper.Map<List<DbTriggerDefinition>>(entities);
            var addedEntities = await base.AddBatchAsync(dbEntities);
            return _mapper.Map<List<Trigger>>(addedEntities);
        }
    }
}