using System.Diagnostics;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
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
            var result = await Edit(device, db);
            stopwatch.Stop();
            Logger.Info($"编辑设备 '{device.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 编辑设备信息。支持事务
    /// </summary>
    /// <param name="device">要编辑的设备对象。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> Edit(Device device, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await db.Updateable<DbDevice>(device.CopyTo<DbDevice>())
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        Logger.Info($"编辑设备 '{device.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
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
                                .Includes(d => d.VariableTables, dvd => dvd.DataVariables, data => data.VariableTable)
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
    /// 删除设备。
    /// </summary>
    /// <param name="id">要删除的设备ID。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> Delete(Device device, List<MenuBean> menus)
    {
        using (var db = DbContext.GetInstance())
        {
            await db.BeginTranAsync();
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var res = await Delete(device, menus, db);

                stopwatch.Stop();
                Logger.Info($"删除设备:{device.Name},耗时：{stopwatch.ElapsedMilliseconds}ms");
                await db.CommitTranAsync();
                return res;
            }
            catch (Exception e)
            {
                await db.RollbackTranAsync();
                throw;
            }

            return 0;
        }
    }


    /// <summary>
    /// 删除设备。
    /// </summary>
    /// <param name="device"></param>
    /// <param name="menus"></param>
    /// <param name="db"></param>
    /// <param name="id">要删除的设备ID。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> Delete(Device device, List<MenuBean> menus, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await db.Deleteable<DbDevice>(new DbDevice { Id = device.Id })
                             .ExecuteCommandAsync();
        // 删除变量表
        await _varTableRepository.Delete(device.VariableTables, db);

        // 删除菜单
        var menu = DataServicesHelper.FindMenusForDevice(device, menus);
        if (menu == null)
            throw new NullReferenceException($"没有找到设备:{device.Name},的菜单对象。");
        await _menuRepository.DeleteMenu(menu, db);
        stopwatch.Stop();
        Logger.Info($"删除设备:{device.Name},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }


    /// <summary>
    /// 添加设备
    /// </summary>
    /// <param name="device"></param>
    public async Task Add(Device device)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var db = DbContext.GetInstance();
        try
        {
            // 开启事务
            await db.BeginTranAsync();
            //查询设备的名字是否存在

            var exist = await db.Queryable<DbDevice>()
                                .Where(d => d.Name == device.Name)
                                .FirstAsync();
            if (exist != null)
                throw new InvalidOperationException("设备名称已经存在。");

            // 2. 将设备添加到数据库
            var addDevice = await Add(device, db);


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
            NotificationHelper.ShowError("添加设备的过程中发生了不可预期的错误：" + e.Message, e);
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
    private async Task<Device> Add(Device device, SqlSugarClient db)
    {
        ;
        // 添加设备
        var addDevice = await db.Insertable<DbDevice>(device.CopyTo<DbDevice>())
                                .ExecuteReturnEntityAsync();

        // 4. 为新设备添加菜单
        var addDeviceMenuId = await _menuRepository.Add(addDevice, db);
        
        if (device.IsAddDefVarTable)
        {
            // 添加默认变量表
            var varTable = new VariableTable();
            device.VariableTables = new List<VariableTable>();
            varTable.IsActive = true;
            varTable.DeviceId = addDevice.Id;
            varTable.Name = "默认变量表";
            varTable.Description = "默认变量表";
            varTable.ProtocolType = device.ProtocolType;
            device.VariableTables.Add(varTable);
            var addVarTable = await _varTableRepository.Add(varTable, db);
            // 添加添加变量表的菜单
            var varTableMenu = new MenuBean()
                               {
                                   Name = "默认变量表",
                                   Icon = SegoeFluentIcons.Tablet.Glyph,
                                   Type = MenuType.VariableTableMenu,
                                   ParentId = addDeviceMenuId,
                                   DataId = addVarTable.Id
                               };
            await _menuRepository.Add(varTableMenu, db);
        }

        await _menuRepository.AddVarTableMenu(addDevice, addDeviceMenuId, db);
        return addDevice.CopyTo<Device>();
    }
}