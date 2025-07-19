using System.Threading.Tasks;

namespace DMS.Core.Interfaces
{
    public interface IUnitOfWork
    {
        Task BeginTranAsync();
        Task CommitTranAsync();
        Task RollbackTranAsync();
    }
}