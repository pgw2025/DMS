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

public class DeviceRepository : BaseRepository<DbDevice, Device>, IDeviceRepository
{
    private readonly IMenuRepository _menuRepository;
    private readonly IVarTableRepository _varTableRepository;

    public DeviceRepository(IMapper mapper, IMenuRepository menuRepository, IVarTableRepository varTableRepository, ITransaction transaction)
        : base(mapper, transaction)
    {
        _menuRepository = menuRepository;
        _varTableRepository = varTableRepository;
    }

    

    public override async Task<List<Device>> GetAllAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var dlist = await Db.Queryable<DbDevice>()
                            .Includes(d => d.VariableTables, dv => dv.Device)
                            .Includes(d => d.VariableTables, dvd => dvd.Variables, data => data.VariableTable)
                            .Includes(d => d.VariableTables, vt => vt.Variables, v => v.VariableMqtts)
                            .ToListAsync();

        stopwatch.Stop();
        NlogHelper.Info($"加载设备列表总耗时：{stopwatch.ElapsedMilliseconds}ms");
        var devices = _mapper.Map<List<Device>>(dlist);
        return devices;
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