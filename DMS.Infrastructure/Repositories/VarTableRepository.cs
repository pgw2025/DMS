using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Interfaces;
using DMS.Infrastructure.Entities;
using System.Diagnostics;
using DMS.Infrastructure.Data;

namespace DMS.Infrastructure.Repositories;

public class VarTableRepository : BaseRepository<DbVariableTable>, IVarTableRepository
{
    public VarTableRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
    }


}