using System.Diagnostics;
using NLog;
using PMSWPF.Data.Entities;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Models;
using SqlSugar;

namespace PMSWPF.Data.Repositories;

public class DeviceRepository
{
    private readonly MenuRepository _menuRepository;
    private readonly VarTableRepository _varTableRepository;
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    public DeviceRepository()
    {
        _menuRepository = new MenuRepository();
        _varTableRepository = new VarTableRepository();
    }


    /// <summary>
    /// 编辑设备信息。
    /// </summary>
    /// <param name="device">要编辑的设备对象。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> Edit(Device device)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await db.Updateable<DbDevice>(device.CopyTo<DbDevice>())
                               .ExecuteCommandAsync();
            stopwatch.Stop();
            Logger.Info($"编辑设备 '{device.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }


    /// <summary>
    /// 获取设备列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<Device>> GetAll()
    {
        using (var db = DbContext.GetInstance())
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var dlist = await db.Queryable<DbDevice>()
                                .Includes(d => d.VariableTables, dv => dv.Device)
                                .Includes(d => d.VariableTables, dvd => dvd.DataVariables ,data=>data.VariableTable)
                                .ToListAsync();
            var devices = new List<Device>();
            foreach (var dbDevice in dlist)
            {
                var device = dbDevice.CopyTo<Device>();
                devices.Add(device);
            }
            stopwatch.Stop();
            Logger.Info($"加载设备列表总耗时：{stopwatch.ElapsedMilliseconds}ms");

            return devices;
        }
    }

    /// <summary>
    /// 根据ID获取设备信息。
    /// </summary>
    /// <param name="id">设备ID。</param>
    /// <returns>对应的DbDevice对象。</returns>
    public async Task<DbDevice> GetById(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await db.Queryable<DbDevice>()
                               .FirstAsync(p => p.Id == id);
            stopwatch.Stop();
            Logger.Info($"根据ID '{id}' 获取设备耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 根据ID删除设备。
    /// </summary>
    /// <param name="id">要删除的设备ID。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteById(int id)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await db.Deleteable<DbDevice>(new DbDevice { Id = id })
                               .ExecuteCommandAsync();
            stopwatch.Stop();
            Logger.Info($"删除设备ID '{id}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 添加设备，包括菜单
    /// </summary>
    /// <param name="device"></param>
    public async Task AddDeviceAndMenu(Device device)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var db = DbContext.GetInstance();
        try
        {
            // 开启事务
            await db.BeginTranAsync();
            // 2. 将设备添加到数据库
            var addDevice = await Add(device, db);
            // 如果数据库添加失败
            if (addDevice == null)
            {
                string addDeviceErrorMsg = $"添加设备失败：{device.Name}";
                Logger.Error(addDeviceErrorMsg);
                NotificationHelper.ShowMessage(addDeviceErrorMsg, NotificationType.Error);
                return; // 提前返回
            }

            // 3. 设备成功添加到数据库，进行菜单添加
            // 这里立即发出成功的通知和日志
            string addDeviceSuccessMsg = $"添加设备成功：{device.Name}";
            Logger.Info(addDeviceSuccessMsg);
            NotificationHelper.ShowMessage(addDeviceSuccessMsg, NotificationType.Success);

            // 4. 为新设备添加菜单
            var addDeviceMenuId = await _menuRepository.AddDeviceMenu(addDevice, db);
            if (device.IsAddDefVarTable)
            {
                var defVarTable = await _varTableRepository.AddDeviceDefVarTable(addDevice, db);
                await _menuRepository.AddDeviceDefTableMenu(device, addDeviceMenuId, defVarTable.Id, db);
            }

            // 添加添加变量表的菜单
            await _menuRepository.AddVarTableMenu(addDevice, addDeviceMenuId, db);
            await db.CommitTranAsync();
            // 菜单也添加成功，通知 UI 更新
            MessageHelper.SendLoadMessage(LoadTypes.Menu);
            MessageHelper.SendLoadMessage(LoadTypes.Devices);
        }
        catch (Exception e)
        {
            // 中间出错了 回滚
            await db.RollbackTranAsync();
            // 捕获并记录所有未预期的异常
            string errorMsg = $"在添加设备过程中发生未预期错误：";
            Logger.Error(errorMsg + e);
            NotificationHelper.ShowMessage(errorMsg + e.Message, NotificationType.Error);
        }
        finally
        {
            stopwatch.Stop();
            Logger.Info($"添加设备 '{device.Name}' 及相关菜单耗时：{stopwatch.ElapsedMilliseconds}ms");
        }
    }

    /// <summary>
    /// 单独添加设备，不包括菜单和变量表
    /// </summary>
    /// <param name="device"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task<DbDevice> Add(Device device, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var exist = await db.Queryable<DbDevice>()
                            .Where(d => d.Name == device.Name)
                            .FirstAsync();
        if (exist != null)
            throw new InvalidOperationException("设备名称已经存在。");
        var dbDevice = new DbDevice();
        device.CopyTo(dbDevice);
        // 是否添加默认变量表
        var result = await db.Insertable<DbDevice>(dbDevice)
                       .ExecuteReturnEntityAsync();
        stopwatch.Stop();
        Logger.Info($"单独添加设备 '{device.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
}