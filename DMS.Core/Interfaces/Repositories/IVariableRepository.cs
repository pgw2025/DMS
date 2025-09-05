
using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories
{
    public interface IVariableRepository:IBaseRepository<Variable>
    {
        /// <summary>
        /// 异步根据OPC UA NodeId获取单个变量实体。
        /// </summary>
        /// <param name="opcUaNodeId">OPC UA NodeId。</param>
        /// <returns>找到的变量实体，如果不存在则返回null。</returns>
        Task<Variable?> GetByOpcUaNodeIdAsync(string opcUaNodeId);

        /// <summary>
        /// 异步根据OPC UA NodeId列表获取变量实体列表。
        /// </summary>
        /// <param name="opcUaNodeIds">OPC UA NodeId列表。</param>
        /// <returns>找到的变量实体列表。</returns>
        Task<List<Variable>> GetByOpcUaNodeIdsAsync(List<string> opcUaNodeIds);
    }
}