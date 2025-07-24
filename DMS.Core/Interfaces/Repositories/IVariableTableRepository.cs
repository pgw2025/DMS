
using DMS.Core.Models;

namespace DMS.Core.Interfaces.Repositories
{
    public interface IVariableTableRepository:IBaseRepository<VariableTable>
    {
        Task<int> DeleteByDeviceIdAsync(int deviceId);
    }
}