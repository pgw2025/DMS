using System.Diagnostics;
using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;


using AutoMapper;
using DMS.Core.Helper;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 用户仓储实现类，负责用户数据的持久化操作。
/// 继承自 <see cref="BaseRepository{DbUser}"/> 并实现 <see cref="IUserRepository"/> 接口。
/// </summary>
public class UserRepository : BaseRepository<DbUser>, IUserRepository
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    public UserRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 异步根据ID获取单个用户。
    /// </summary>
    /// <param name="id">用户的唯一标识符。</param>
    /// <returns>对应的用户实体，如果不存在则为null。</returns>
    public async Task<User> GetByIdAsync(int id)
    {
        var dbUser = await base.GetByIdAsync(id);
        return _mapper.Map<User>(dbUser);
    }

    /// <summary>
    /// 异步获取所有用户。
    /// </summary>
    /// <returns>包含所有用户实体的列表。</returns>
    public async Task<List<User>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<User>>(dbList);
    }

    /// <summary>
    /// 异步添加新用户。
    /// </summary>
    /// <param name="entity">要添加的用户实体。</param>
    /// <returns>添加成功后的用户实体（包含数据库生成的ID等信息）。</returns>
    public async Task<User> AddAsync(User entity)
    {
        var dbUser = await base.AddAsync(_mapper.Map<DbUser>(entity));
        return _mapper.Map(dbUser, entity);
    }

    /// <summary>
    /// 异步更新现有用户。
    /// </summary>
    /// <param name="entity">要更新的用户实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAsync(User entity) => await base.UpdateAsync(_mapper.Map<DbUser>(entity));

    /// <summary>
    /// 异步删除用户。
    /// </summary>
    /// <param name="entity">要删除的用户实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(User entity) => await base.DeleteAsync(_mapper.Map<DbUser>(entity));
    
    /// <summary>
    /// 异步根据ID删除用户。
    /// </summary>
    /// <param name="id">要删除用户的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new DbUser() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbUser)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
    
    /// <summary>
    /// 异步获取指定数量的用户。
    /// </summary>
    /// <param name="number">要获取的用户数量。</param>
    /// <returns>包含指定数量用户实体的列表。</returns>
    public new async Task<List<User>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<User>>(dbList);

    }
}