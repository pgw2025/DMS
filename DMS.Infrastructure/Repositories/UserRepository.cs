using System.Diagnostics;
using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;


/// <summary>
///     用户仓储类，用于操作DbUser实体
/// </summary>
using AutoMapper;
using DMS.Core.Helper;
using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

/// <summary>
///     用户仓储类，用于操作DbUser实体
/// </summary>
public class UserRepository : BaseRepository<DbUser>, IUserRepository
{
    private readonly IMapper _mapper;

    public UserRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
    {
        _mapper = mapper;
    }

    public async Task<User> GetByIdAsync(int id)
    {
        var dbUser = await base.GetByIdAsync(id);
        return _mapper.Map<User>(dbUser);
    }

    public async Task<List<User>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<User>>(dbList);
    }

    public async Task<User> AddAsync(User entity)
    {
        var dbUser = await base.AddAsync(_mapper.Map<DbUser>(entity));
        return _mapper.Map(dbUser, entity);
    }

    public async Task<int> UpdateAsync(User entity) => await base.UpdateAsync(_mapper.Map<DbUser>(entity));

    public async Task<int> DeleteAsync(User entity) => await base.DeleteAsync(_mapper.Map<DbUser>(entity));
    
    public async Task<int> DeleteAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new User() { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbMenu)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
}