namespace DMS.Core.Interfaces.Repositories;

/// <summary>
/// 提供泛型数据访问操作的基础仓储接口。
/// </summary>
/// <typeparam name="T">领域模型的类型。</typeparam>
public interface IBaseRepository<T> where T : class
{
    /// <summary>
    /// 异步根据ID获取单个实体。
    /// </summary>
    /// <param name="id">实体的主键ID。</param>
    /// <returns>找到的实体，如果不存在则返回null。</returns>
    Task<T> GetByIdAsync(int id);

    /// <summary>
    /// 异步获取所有实体。
    /// </summary>
    /// <returns>所有实体的列表。</returns>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// 异步添加一个新实体。
    /// </summary>
    /// <param name="entity">要添加的实体。</param>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// 异步更新一个已存在的实体。
    /// </summary>
    /// <param name="entity">要更新的实体。</param>
    Task<int> UpdateAsync(T entity);

    /// <summary>
    /// 异步根据ID删除一个实体。
    /// </summary>
    /// <param name="id">要删除的实体的主键ID。</param>
    Task<int> DeleteAsync(T entity);
}