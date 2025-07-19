using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Interfaces;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Data;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 用户仓储类，用于操作DbUser实体
/// </summary>
public class UserRepository : BaseRepository<DbUser>
{
    public UserRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
    }
}