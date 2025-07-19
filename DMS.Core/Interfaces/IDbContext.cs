using System.Threading.Tasks;

namespace DMS.Core.Interfaces
{
    public interface IDbContext
    {
        // Define common database operations here, e.g.,
        // Task<TEntity> GetByIdAsync<TEntity>(int id) where TEntity : class;
        // Task AddAsync<TEntity>(TEntity entity) where TEntity : class;
        // Task UpdateAsync<TEntity>(TEntity entity) where TEntity : class;
        // Task DeleteAsync<TEntity>(TEntity entity) where TEntity : class;
        // SqlSugarClient GetClient(); // This should NOT be here if you want to hide SqlSugar

        // For now, we'll just keep it empty or add methods as needed.
        // The primary goal is to abstract away SqlSugarClient.
        // The IUnitOfWork already handles transactions.
    }
}