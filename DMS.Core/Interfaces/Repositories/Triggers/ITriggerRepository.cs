using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Core.Models.Triggers;

namespace DMS.Core.Interfaces.Repositories.Triggers
{
    /// <summary>
    /// 触发器仓储接口 (定义对 TriggerDefinition 实体的数据访问方法)
    /// </summary>
    public interface ITriggerRepository
    {
        /// <summary>
        /// 获取所有触发器定义
        /// </summary>
        /// <returns>触发器定义实体列表</returns>
        Task<IEnumerable<TriggerDefinition>> GetAllAsync();

        /// <summary>
        /// 根据 ID 获取触发器定义
        /// </summary>
        /// <param name="id">触发器 ID</param>
        /// <returns>触发器定义实体，如果未找到则返回 null</returns>
        Task<TriggerDefinition?> GetByIdAsync(int id);

        /// <summary>
        /// 添加一个新的触发器定义
        /// </summary>
        /// <param name="trigger">要添加的触发器定义实体</param>
        /// <returns>添加成功的触发器定义实体（通常会填充生成的 ID）</returns>
        Task<TriggerDefinition> AddAsync(TriggerDefinition trigger);

        /// <summary>
        /// 更新一个已存在的触发器定义
        /// </summary>
        /// <param name="trigger">包含更新信息的触发器定义实体</param>
        /// <returns>更新后的触发器定义实体，如果未找到则返回 null</returns>
        Task<TriggerDefinition?> UpdateAsync(TriggerDefinition trigger);

        /// <summary>
        /// 删除一个触发器定义
        /// </summary>
        /// <param name="id">要删除的触发器 ID</param>
        /// <returns>删除成功返回 true，否则返回 false</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// 获取与指定变量关联的所有触发器定义
        /// </summary>
        /// <param name="variableId">变量 ID</param>
        /// <returns>该变量关联的触发器定义实体列表</returns>
        Task<IEnumerable<TriggerDefinition>> GetByVariableIdAsync(int variableId);
    }
}