using AutoMapper;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services
{
    public class MqttService:BaseService<MqttServer, MqttServerRepository>
    {
        public MqttService( MqttServerRepository serverRepository) : base( serverRepository)
        {
        }
    }
}
