using System.Diagnostics;
using AutoMapper;
using DMS.Infrastructure.Entities;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using SqlSugar;

namespace DMS.Infrastructure.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private readonly IMapper _mapper;
    private readonly IMenuRepository _menuRepository;
    private readonly IVarTableRepository _varTableRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeviceRepository(IMapper mapper, IMenuRepository menuRepository, IVarTableRepository varTableRepository, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _menuRepository = menuRepository;
        _varTableRepository = varTableRepository;
        _unitOfWork = unitOfWork;
    }



    /// <summary>
    /// 编辑设备信息。
    /// </summary>
    /// <param name="device">要编辑的设备对象。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAsync(Device device)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var db = _unitOfWork.GetInstance();
        var result = await db.Updateable<DbDevice>(_mapper.Map<DbDevice>(device))
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"编辑设备 '{device.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    


    /// <summary>
    /// 获取设备列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<Device>> GetAllAsync()
    {
        using (var db = DbContext.GetInstance())
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var dlist = await db.Queryable<DbDevice>()
                                .Includes(d => d.VariableTables, dv => dv.Device)
                                .Includes(d => d.VariableTables, dvd => dvd.Variables, data => data.VariableTable)
                                .Includes(d => d.VariableTables, vt => vt.Variables, v => v.VariableMqtts)
                                .ToListAsync();


            stopwatch.Stop();
            NlogHelper.Info($"加载设备列表总耗时：{stopwatch.ElapsedMilliseconds}ms");
            var devices = _mapper.Map<List<Device>>(dlist);
            return devices;
        }
    }

    /// <summary>
    /// 根据ID获取设备信息。
    /// </summary>
    /// <param name="id">设备ID。</param>
    /// <returns>对应的DbDevice对象。</returns>
    public async Task<Device> GetByIdAsync(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await db.Queryable<DbDevice>()
                                 .FirstAsync(p => p.Id == id);
            stopwatch.Stop();
            NlogHelper.Info($"根据ID '{id}' 获取设备耗时：{stopwatch.ElapsedMilliseconds}ms");
            return _mapper.Map<Device>(result);
        }
    }


    /// <summary>
    /// 删除设备。
    /// </summary>
    /// <param name="id">要删除的设备ID。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(Device device, List<MenuBean> menus)
    {
        var db = _unitOfWork.GetInstance();
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await db.Deleteable<DbDevice>(new DbDevice { Id = device.Id })
                             .ExecuteCommandAsync();
        // 删除变量表
        await _varTableRepository.DeleteAsync(device.VariableTables);

        stopwatch.Stop();
        NlogHelper.Info($"删除设备:{device.Name},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }


    


    /// <summary>
    /// 添加设备
    /// </summary>
    /// <param name="device"></param>
    public async Task AddAsync(Device device)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var db = _unitOfWork.GetInstance();

        //查询设备的名字是否存在
        var exist = await db.Queryable<DbDevice>()
                            .Where(d => d.Name == device.Name)
                            .FirstAsync();
        if (exist != null)
            throw new InvalidOperationException("设备名称已经存在。");

        // 2. 将设备添加到数据库
        var addDevice = await db.Insertable<DbDevice>(_mapper.Map<DbDevice>(device))
                                .ExecuteReturnEntityAsync();

        // 4. 为新设备添加菜单
        var addDeviceMenuId = await _menuRepository.AddAsync(addDevice);

        stopwatch.Stop();
        NlogHelper.Info($"添加设备 '{device.Name}' 及相关菜单耗时：{stopwatch.ElapsedMilliseconds}ms");
    }


}