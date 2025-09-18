
using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories
{
    public interface IVariableTableRepository:IBaseRepository<VariableTable>
    {
        Task<int> DeleteByDeviceIdAsync(int deviceId);

        /// <summary>
        /// 异步根据ID获取单个变量表。
        /// </summary>
        /// <param name="id">变量表的唯一标识符。</param>
        /// <returns>对应的变量表实体，如果不存在则为null。</returns>
        Task<List<VariableTable>> GetByDeviceIdAsync(int deviceId);
    }
}