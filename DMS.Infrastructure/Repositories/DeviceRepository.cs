using System.Diagnostics;
using AutoMapper;
using DMS.Infrastructure.Entities;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using SqlSugar;
using DMS.Infrastructure.Interfaces;

namespace DMS.Infrastructure.Repositories;

public class DeviceRepository : BaseRepository<DbDevice>
{

    public DeviceRepository(ITransaction transaction)
        : base(transaction)
    {
        
    }

    
}