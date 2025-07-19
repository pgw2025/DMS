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

namespace DMS.Infrastructure.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly DeviceRepository _deviceRepository;
        private readonly IMenuRepository _menuRepository;
        private readonly IVarTableRepository _varTableRepository;
        private readonly IMapper _mapper;
        private readonly SqlSugarClient Db; // Assuming DbContext is accessible or passed

        public DeviceService(DeviceRepository deviceRepository, IMenuRepository menuRepository, IVarTableRepository varTableRepository, IMapper mapper, SqlSugarDbContext dbContext)
        {
            _deviceRepository = deviceRepository;
            _menuRepository = menuRepository;
            _varTableRepository = varTableRepository;
            _mapper = mapper;
            Db = dbContext.GetInstance();
        }

        public async Task<int> DeleteAsync(Device device, List<MenuBean> menus)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var result = await Db.Deleteable<DbDevice>(new DbDevice { Id = device.Id })
                                 .ExecuteCommandAsync();
            // 删除变量表
            //await _varTableRepository.DeleteAsync(device.VariableTables);

            stopwatch.Stop();
            NlogHelper.Info($"删除设备:{device.Name},耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }

        public async Task AddAsync(Device device)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //查询设备的名字是否存在
            var exist = await Db.Queryable<DbDevice>()
                                .Where(d => d.Name == device.Name)
                                .FirstAsync();
            if (exist != null)
                throw new InvalidOperationException("设备名称已经存在。");

            // 2. 将设备添加到数据库
            var addDevice = await Db.Insertable<DbDevice>(_mapper.Map<DbDevice>(device))
                                    .ExecuteReturnEntityAsync();

            // 4. 为新设备添加菜单
            //var addDeviceMenuId = await _menuRepository.AddAsync(addDevice);

            stopwatch.Stop();
            NlogHelper.Info($"添加设备 '{device.Name}' 及相关菜单耗时：{stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
