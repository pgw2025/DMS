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
    public class DeviceService : IDeviceService
    {
        private readonly DeviceRepository _deviceRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly IVarTableRepository _varTableRepository;
        private readonly IMapper _mapper;

        private ConcurrentDictionary<int, Device> _devicesDic;

        public DeviceService(DeviceRepository deviceRepository, IMenuRepository menuRepository, IVarTableRepository varTableRepository, IMapper mapper, SqlSugarDbContext dbContext)
        {
            _deviceRepository = deviceRepository;
            _menuRepository = menuRepository;
            _varTableRepository = varTableRepository;
            _mapper = mapper;
            _devicesDic = new ConcurrentDictionary<int, Device>();
        }

        public async Task<List<Device>> GetAllAsync()
        {
            var dbDevices = await _deviceRepository.GetAllAsync();

            var deviceDic = _mapper.Map<List<Device>>(dbDevices).ToDictionary(d => d.Id);
            _devicesDic = new ConcurrentDictionary<int, Device>(deviceDic);

            return deviceDic.Values.ToList();
        }


        public async Task<Device> AddAsync(Device device)
        {
            Device resDevice = null;
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //查询设备的名字是否存在
                if (_devicesDic.Values.Any(d => d.Name == device.Name || (d.Ip == device.Ip && d.Prot == device.Prot) || d.OpcUaEndpointUrl == device.OpcUaEndpointUrl))
                {
                    NlogHelper.Warn("设备的名称，Ip:端口，OpcUrl,不可以重复。");
                    return resDevice;
                }
                await _deviceRepository.GetTransaction().BeginTranAsync();
                // 2. 将设备添加到数据库
                var addDevice = await _deviceRepository.AddAsync(_mapper.Map<DbDevice>(device));

                //判断判断是否添加默认变量表
                if (device.IsAddDefVarTable)
                {
                    DbVariableTable dbVariableTable = new DbVariableTable();
                    dbVariableTable.Name = "默认变量表";
                    dbVariableTable.Description = "默认变量表";
                    dbVariableTable.DeviceId = addDevice.Id;
                    dbVariableTable.ProtocolType = addDevice.ProtocolType;
                   var dbAddVarTable= await _varTableRepository.AddAsync(dbVariableTable);
                    if (addDevice.VariableTables==null)
                    {
                        addDevice.VariableTables= new List<DbVariableTable>(); 
                    }

                    addDevice.VariableTables.Add(dbAddVarTable);
                }

                // 4. 为新设备添加菜单
                //var addDeviceMenuId = await _menuRepository.AddAsync(addDevice);
                resDevice = _mapper.Map<Device>(addDevice);

                await _deviceRepository.GetTransaction().CommitTranAsync();

                stopwatch.Stop();
                NlogHelper.Info($"添加设备 '{device.Name}' 及相关菜单耗时：{stopwatch.ElapsedMilliseconds}ms");
                return resDevice;
            }
            catch (Exception e)
            {
                await _deviceRepository.GetTransaction().RollbackTranAsync();
                throw;
                
            }

        }
    }
}
