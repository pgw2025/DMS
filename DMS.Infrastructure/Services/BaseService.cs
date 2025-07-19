using AutoMapper;
using DMS.Infrastructure.Repositories;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services
{
    /// <summary>
    /// 通用服务基类，封装了常见的增、删、改操作。
    /// </summary>
    /// <typeparam name="TModel">业务逻辑模型类型。</typeparam>
    /// <typeparam name="TEntity">数据库实体类型。</typeparam>
    /// <typeparam name="TRepository">与实体对应的仓储类型。</typeparam>
    public abstract class BaseService<TModel, TEntity, TRepository>
        where TEntity : class, new()
        where TRepository : BaseRepository<TEntity>
    {
        protected readonly IMapper _mapper;
        protected readonly TRepository _repository;

        /// <summary>
        /// 初始化 BaseService 的新实例。
        /// </summary>
        /// <param name="mapper">AutoMapper 实例，用于对象映射。</param>
        /// <param name="repository">仓储实例，用于数据访问。</param>
        protected BaseService(IMapper mapper, TRepository repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        /// <summary>
        /// 异步添加一个新的业务模型对象。
        /// </summary>
        /// <param name="model">要添加的业务模型对象。</param>
        /// <returns>返回添加后的数据库实体。</returns>
        public virtual async Task<TEntity> AddAsync(TModel model)
        {
            var entity = _mapper.Map<TEntity>(model);
            return await _repository.AddAsync(entity);
        }

        /// <summary>
        /// 异步更新一个现有的业务模型对象。
        /// </summary>
        /// <param name="model">要更新的业务模型对象。</param>
        /// <returns>返回受影响的行数。</returns>
        public virtual async Task<int> UpdateAsync(TModel model)
        {
            var entity = _mapper.Map<TEntity>(model);
            return await _repository.UpdateAsync(entity);
        }

        /// <summary>
        /// 异步删除一个业务模型对象。
        /// </summary>
        /// <param name="model">要删除的业务模型对象。</param>
        /// <returns>返回受影响的行数。</returns>
        public virtual async Task<int> DeleteAsync(TModel model)
        {
            var entity = _mapper.Map<TEntity>(model);
            return await _repository.DeleteAsync(entity);
        }
    }
}
