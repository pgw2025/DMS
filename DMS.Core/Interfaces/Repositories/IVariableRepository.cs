
using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories
{
    public interface IVariableRepository:IBaseRepository<Variable>
    {
        /// <summary>
        /// 异步根据变量表ID删除变量。
        /// </summary>
        /// <param name="variableTableId">变量表的唯一标识符。</param>
        /// <returns>受影响的行数。</returns>
        Task<int> DeleteByVariableTableIdAsync(int variableTableId);
        
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

        /// <summary>
        /// 异步批量更新变量。
        /// </summary>
        /// <param name="variables">要更新的变量实体集合。</param>
        /// <returns>受影响的行数。</returns>
        Task<int> UpdateBatchAsync(IEnumerable<Variable> variables);
    }
}