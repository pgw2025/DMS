
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.Core.Interfaces.Repositories
{
    public interface IMqttAliasRepository : IBaseRepository<MqttAlias>
    {

        /// <summary>
        /// 异步获取所有变量与MQTT别名关联。
        /// </summary>
        /// <returns>包含所有变量与MQTT别名关联实体的列表。</returns>
        Task<List<MqttAlias>> GetAllAsync();
    }
}