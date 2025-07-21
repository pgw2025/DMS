using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IVariableTableRepository : IBaseRepository<VariableTable>
{
    // 可以添加特定于VariableTable的查询方法
}