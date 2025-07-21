using AutoMapper;
using DMS.Infrastructure.Repositories;
using System.Threading.Tasks;
using DMS.Core.Interfaces.Repositories;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// 通用服务基类，封装了常见的增、删、改操作。
    /// </summary>
    /// <typeparam name="TModel">业务逻辑模型类型。</typeparam>
    /// <typeparam name="TEntity">数据库实体类型。</typeparam>
    /// <typeparam name="TRepository">与实体对应的仓储类型。</typeparam>
    public abstract class BaseService<TModel, TRepository>
        where TModel : class, new()
        where TRepository : IBaseRepository<TModel>
    {
        protected readonly TRepository ServerRepository;

        /// <summary>
        /// 初始化 BaseService 的新实例。
        /// </summary>
        /// <param name="mapper">AutoMapper 实例，用于对象映射。</param>
        /// <param name="serverRepository">仓储实例，用于数据访问。</param>
        protected BaseService( TRepository serverRepository)
        {
            ServerRepository = serverRepository;
        }

        /// <summary>
        /// 异步添加一个新的业务模型对象。
        /// </summary>
        /// <param name="model">要添加的业务模型对象。</param>
        /// <returns>返回添加后的数据库实体。</returns>
        public virtual async Task<TModel> AddAsync(TModel model)
        {
            return await ServerRepository.AddAsync(model);
        }

        /// <summary>
        /// 异步更新一个现有的业务模型对象。
        /// </summary>
        /// <param name="model">要更新的业务模型对象。</param>
        /// <returns>返回受影响的行数。</returns>
        public virtual async Task<int> UpdateAsync(TModel model)
        {
            return await ServerRepository.UpdateAsync(model);
        }

        /// <summary>
        /// 异步删除一个业务模型对象。
        /// </summary>
        /// <param name="model">要删除的业务模型对象。</param>
        /// <returns>返回受影响的行数。</returns>
        public virtual async Task<int> DeleteAsync(TModel model)
        {
            return await ServerRepository.DeleteAsync(model);
        }
        
        public virtual async Task<List<TModel>> GetAllAsync()
        {
            return await ServerRepository.GetAllAsync();
        }
        
        public virtual async Task<TModel> GetByIdAsync(int id)
        {
            return await ServerRepository.GetByIdAsync(id);
        }
    }
}
