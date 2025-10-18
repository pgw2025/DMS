using DMS.Core.Models.Triggers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Application.Interfaces.Database
{
    /// <summary>
    /// 触发器应用服务接口，负责处理触发器相关的业务逻辑。
    /// </summary>
    public interface ITriggerAppService
    {
        /// <summary>
        /// 异步根据ID获取触发器定义。
        /// </summary>
        /// <param name="id">触发器ID。</param>
        /// <returns>触发器定义实体。</returns>
        Task<Trigger> GetTriggerByIdAsync(int id);

        /// <summary>
        /// 异步获取所有触发器定义。
        /// </summary>
        /// <returns>触发器定义实体列表。</returns>
        Task<List<Trigger>> GetAllTriggersAsync();

        /// <summary>
        /// 异步创建一个新触发器定义及其关联的变量ID。
        /// </summary>
        /// <param name="trigger">要创建的触发器定义。</param>
        /// <returns>新创建的触发器定义。</returns>
        Task<Trigger> CreateTriggerAsync(Trigger trigger);

        /// <summary>
        /// 异步更新一个已存在的触发器定义及其关联的变量ID。
        /// </summary>
        /// <param name="trigger">要更新的触发器定义。</param>
        /// <returns>受影响的行数。</returns>
        Task<int> UpdateTriggerAsync(Trigger trigger);

        /// <summary>
        /// 异步删除一个触发器定义及其关联的变量关系。
        /// </summary>
        /// <param name="id">要删除的触发器ID。</param>
        /// <returns>如果删除成功则为 true，否则为 false。</returns>
        Task<bool> DeleteTriggerByIdAsync(int id);

        /// <summary>
        /// 异步获取指定变量ID关联的所有触发器定义。
        /// </summary>
        /// <param name="variableId">变量ID。</param>
        /// <returns>与指定变量关联的触发器定义实体列表。</returns>
        Task<IEnumerable<Trigger>> GetTriggersByVariableIdAsync(int variableId);
    }
}