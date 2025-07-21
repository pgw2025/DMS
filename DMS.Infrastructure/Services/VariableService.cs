using AutoMapper;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DMS.Infrastructure.Repositories;

namespace DMS.Infrastructure.Services
{
    public class VariableService : BaseService<Variable, VariableRepository>
    {

        public VariableService(VariableRepository repository) : base(repository)
        {
        }

    }
}
