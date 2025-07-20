using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;

namespace DMS.Infrastructure.Services;

public class VarTableService : BaseService<VariableTable, DbVariableTable, VarTableRepository>
{
    public VarTableService(IMapper mapper, VarTableRepository repository) : base(mapper, repository)
    {
    }
}