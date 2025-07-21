using AutoMapper;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using DMS.Core.Interfaces;
using DMS.Core.Interfaces.Repositories;

namespace DMS.Infrastructure.Services
{
    public class DeviceService : BaseService<Device, DeviceRepository>
    {
        private readonly IDeviceRepository _deviceRepository;

        public DeviceService(DeviceRepository repository) : base(repository)
        {
            _deviceRepository = repository;
        }

    }
}