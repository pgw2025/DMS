using AutoMapper;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using DMS.Infrastructure.Repositories;
using DMS.Infrastructure.Interfaces;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace DMS.Infrastructure.Services
{
    public class DeviceService  :BaseService<Device, DbDevice, DeviceRepository>
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly IVarTableRepository _varTableRepository;
        private readonly IMapper _mapper;


        public DeviceService(IMapper mapper, DeviceRepository repository) : base(mapper, repository)
        {
            _deviceRepository = repository;
            _mapper = mapper;
        }

        public async Task<List<Device>> GetAllAsync()
        {
            var dbDevices = await _deviceRepository.GetAllAsync();

            return _mapper.Map<List<Device>>(dbDevices);
        }


        public async Task<Device> AddAsync(Device device)
        {
            Device resDevice = null;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var dbList = await GetAllAsync();
            //查询设备的名字是否存在
            if (dbList.Any(d => d.Name == device.Name || (d.Ip == device.Ip && d.Prot == device.Prot) || d.OpcUaEndpointUrl == device.OpcUaEndpointUrl))
            {
                NlogHelper.Warn("设备的名称，Ip:端口，OpcUrl,不可以重复。");
                return resDevice;
            }
            // 2. 将设备添加到数据库
            var addDevice = await _deviceRepository.AddAsync(_mapper.Map<DbDevice>(device));

            //判断判断是否添加默认变量表
            //if (device.IsAddDefVarTable)
            //{
            //    DbVariableTable dbVariableTable = new DbVariableTable();
            //    dbVariableTable.Name = "默认变量表";
            //    dbVariableTable.Description = "默认变量表";
            //    dbVariableTable.DeviceId = addDevice.Id;
            //    dbVariableTable.ProtocolType = addDevice.ProtocolType;
            //   var dbAddVarTable= await _varTableRepository.AddAsync(dbVariableTable);
            //    if (addDevice.VariableTables==null)
            //    {
            //        addDevice.VariableTables= new List<DbVariableTable>(); 
            //    }

            //    addDevice.VariableTables.Add(dbAddVarTable);
            //}

            // 4. 为新设备添加菜单
            //var addDeviceMenuId = await _menuRepository.AddAsync(addDevice);
            resDevice = _mapper.Map<Device>(addDevice);


            stopwatch.Stop();
            NlogHelper.Info($"添加设备 '{device.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return resDevice;

        }
    }
}
