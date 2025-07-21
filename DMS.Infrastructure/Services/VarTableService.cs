using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;

namespace DMS.Infrastructure.Services;

public class VarTableService : BaseService<VariableTable, DbVariableTable, VariableTableRepository>
{
    public VarTableService(IMapper mapper, VariableTableRepository repository) : base(mapper, repository)
    {
    }
}