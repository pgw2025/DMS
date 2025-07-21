using DMS.Core.Models;

namespace DMS.Core.Interfaces;

public interface IVariableHistoryRepository : IBaseRepository<VariableHistory>
{
    // 可以添加特定于VariableHistory的查询方法
}