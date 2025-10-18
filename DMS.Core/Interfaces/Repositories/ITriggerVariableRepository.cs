using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.Core.Models.Triggers;

namespace DMS.Core.Interfaces.Repositories
{
    public interface ITriggerVariableRepository : IBaseRepository<TriggerVariable>
    {
        /// <summary>
        /// 异步获取所有触发器与变量关联。
        /// </summary>
        /// <returns>包含所有触发器与变量关联实体的列表。</returns>
        Task<List<TriggerVariable>> GetAllAsync();
    }
}