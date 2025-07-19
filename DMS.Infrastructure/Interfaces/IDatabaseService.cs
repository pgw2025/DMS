using System.Threading.Tasks;

namespace DMS.Infrastructure.Interfaces
{
    public interface IDatabaseService
    {
        void InitializeDataBase();
        Task InitializeMenu();
    }
}