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
        _menuRepository=new MenuRepository();
        _varTableRepository=new VarTableRepository();
    }

   

    
    public async Task<int> Edit(Device device)
    {
        using (var db = DbContext.GetInstance())
        {
            return await db.Updateable<DbDevice>(device.CopyTo<DbDevice>()).ExecuteCommandAsync();
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
            var dlist = await db.Queryable<DbDevice>().Includes(d => d.VariableTables, dv => dv.Device).ToListAsync();
            var devices = new List<Device>();
            foreach (var dbDevice in dlist)
            {
                var device = dbDevice.CopyTo<Device>();
                devices.Add(device);
            }

            return devices;
        }
    }

    public async Task<DbDevice> GetById(int id)
    {
        using (var db = DbContext.GetInstance())
        {
            return await db.Queryable<DbDevice>().FirstAsync(p => p.Id == id);
        }
    }

    public async Task<int> DeleteById(int id)
    {
        using (var db = DbContext.GetInstance())
        {
            return await db.Deleteable<DbDevice>(new DbDevice { Id = id }).ExecuteCommandAsync();
        }
    }
    
    /// <summary>
    /// 添加设备，包括菜单
    /// </summary>
    /// <param name="device"></param>
    public async Task AddDeviceAndMenu(Device device)
    {
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
                await _menuRepository.AddDeviceDefTableMenu(device, addDeviceMenuId, defVarTable.Id,db);
            }

            // 添加添加变量表的菜单
            await _menuRepository.AddVarTableMenu(addDevice,addDeviceMenuId, db);
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
            Logger.Error(errorMsg+e);
            NotificationHelper.ShowMessage(errorMsg+e.Message, NotificationType.Error);

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
        var exist = await db.Queryable<DbDevice>().Where(d => d.Name == device.Name).FirstAsync();
        if (exist != null)
            throw new InvalidOperationException("设备名称已经存在。");
        var dbDevice = new DbDevice();
        device.CopyTo(dbDevice);
        // 是否添加默认变量表
        return await db.Insertable<DbDevice>(dbDevice).ExecuteReturnEntityAsync();
    }
}