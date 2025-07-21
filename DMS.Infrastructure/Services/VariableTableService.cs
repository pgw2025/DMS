using AutoMapper;
using DMS.Core.Models;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;

namespace DMS.Infrastructure.Services;

public class VariableTableService : BaseService<VariableTable, VariableTableRepository>
{
    public VariableTableService(VariableTableRepository repository) : base(repository)
    {
    }
}