using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Core.Models.Triggers;
using SqlSugar;

namespace DMS.Infrastructure.Repositories.Triggers.Impl
{
    /// <summary>
    /// 基于 SqlSugar 的触发器仓储实现
    /// </summary>
    public class SqlSugarTriggerRepository : ITriggerRepository
    {
        private readonly ISqlSugarClient _db;

        public SqlSugarTriggerRepository(ISqlSugarClient db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// 获取所有触发器定义
        /// </summary>
        public async Task<IEnumerable<TriggerDefinition>> GetAllAsync()
        {
            return await _db.Queryable<TriggerDefinition>().ToListAsync();
        }

        /// <summary>
        /// 根据 ID 获取触发器定义
        /// </summary>
        public async Task<TriggerDefinition?> GetByIdAsync(Guid id)
        {
            return await _db.Queryable<TriggerDefinition>().InSingleAsync(id);
        }

        /// <summary>
        /// 添加一个新的触发器定义
        /// </summary>
        public async Task<TriggerDefinition> AddAsync(TriggerDefinition trigger)
        {
            var insertedId = await _db.Insertable(trigger).ExecuteReturnSnowflakeIdAsync();
            trigger.Id = insertedId;
            return trigger;
        }

        /// <summary>
        /// 更新一个已存在的触发器定义
        /// </summary>
        public async Task<TriggerDefinition?> UpdateAsync(TriggerDefinition trigger)
        {
            var rowsAffected = await _db.Updateable(trigger).ExecuteCommandAsync();
            return rowsAffected > 0 ? trigger : null;
        }

        /// <summary>
        /// 删除一个触发器定义
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            var rowsAffected = await _db.Deleteable<TriggerDefinition>().In(id).ExecuteCommandAsync();
            return rowsAffected > 0;
        }

        /// <summary>
        /// 获取与指定变量关联的所有触发器定义
        /// </summary>
        public async Task<IEnumerable<TriggerDefinition>> GetByVariableIdAsync(Guid variableId)
        {
            return await _db.Queryable<TriggerDefinition>()
                            .Where(t => t.VariableId == variableId)
                            .ToListAsync();
        }
    }
}