using System.Diagnostics;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using NLog;
using PMSWPF.Data.Entities;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Models;
using SqlSugar;

namespace PMSWPF.Data.Repositories;

public class MenuRepository
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public MenuRepository()
    {
    }

    public async Task<int> DeleteMenu(MenuBean menu)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var childList = await db.Queryable<DbMenu>().ToChildListAsync(it => it.ParentId, menu.Id);
            var result = await db.Deleteable<DbMenu>(childList).ExecuteCommandAsync();
            stopwatch.Stop();
            // Assuming NLog is available and configured for MenuRepository
            // If not, you might need to add a Logger field similar to DeviceRepository
            // For now, I'll assume it's available or will be added.
            Logger.Info($"删除菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    public async Task<List<MenuBean>> GetMenuTrees()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            List<MenuBean> menuTree = new();
            var dbMenuTree = await db.Queryable<DbMenu>().ToTreeAsync(dm => dm.Items, dm => dm.ParentId, 0);

            foreach (var dbMenu in dbMenuTree)
                menuTree.Add(dbMenu.CopyTo<MenuBean>());
            stopwatch.Stop();
            Logger.Info($"获取菜单树耗时：{stopwatch.ElapsedMilliseconds}ms");
            return menuTree;
        }
    }


    public async Task<int> Add(MenuBean menu)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await db.Insertable<DbMenu>(menu.CopyTo<DbMenu>()).ExecuteCommandAsync();
            stopwatch.Stop();
            Logger.Info($"添加菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
    
    /// <summary>
    /// 添加默认变量表的菜单
    /// </summary>
    /// <param name="device"></param>
    /// <param name="addDeviceMenuId"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<int> AddDeviceDefTableMenu(Device device, int parentMenuId,int varTableId, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var defVarTableMenu = new MenuBean()
        {
            Name = "默认变量表",
            Icon = SegoeFluentIcons.Tablet.Glyph,
            Type = MenuType.VariableTableMenu,
            ParentId = parentMenuId,
            DataId = varTableId
        };
        var defTableRes = await db.Insertable<DbMenu>(defVarTableMenu).ExecuteCommandAsync();
        stopwatch.Stop();
        Logger.Info($"添加默认变量表菜单 '{defVarTableMenu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return defTableRes;
    }

    /// <summary>
    /// 给设备添加默认变量表菜单
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    // public async Task<bool> AddDeviceDefVarTableMenu(Device device)
    // {
    //     var db = DbContext.GetInstance();
    //     try
    //     {
    //         await db.BeginTranAsync();
    //         bool result = false;
    //         var parentMenuId = await AddDeviceMenu(device, db);
    //         var defTableRes = await AddDeviceDefTableMenu(device, parentMenuId, db);
    //         var addTableRes = await AddVarTableMenu(parentMenuId, db);
    //         // if ((addTableRes + defTableRes) != 2)
    //         // {
    //         //     // 如果出错删除原来添加的设备菜单
    //         //     await db.Deleteable<DbMenu>().Where(m => m.Id == parentMenuId).ExecuteCommandAsync();
    //         //     throw new InvalidOperationException("添加默认变量表时发生了错误！！");
    //         // }
    //
    //         await db.CommitTranAsync();
    //         return true;
    //     }
    //     catch (Exception e)
    //     {
    //         await db.RollbackTranAsync();
    //     }
    //     finally
    //     {
    //     }
    // }

    public async Task<int> AddVarTableMenu(DbDevice dbDevice, int parentMenuId, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var addVarTable = new MenuBean()
        {
            Name = "添加变量表",
            Icon = SegoeFluentIcons.Add.Glyph,
            Type = MenuType.AddVariableTableMenu,
            ParentId = parentMenuId,
            DataId = dbDevice.Id
        };
        var addTableRes = await db.Insertable<DbMenu>(addVarTable).ExecuteCommandAsync();
        stopwatch.Stop();
        // Logger.Info($"添加变量表菜单 '{addVarTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return addTableRes;
    }

   
    /// <summary>
    /// 添加设备菜单
    /// </summary>
    /// <param name="device"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<int> AddDeviceMenu(DbDevice device, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var deviceMainMenu = await db.Queryable<DbMenu>().FirstAsync(m => m.Name == "设备");
        if (deviceMainMenu == null)
            throw new InvalidOperationException("没有找到设备菜单！！");

        // 添加菜单项
        MenuBean menu = new MenuBean()
        {
            Name = device.Name,
            Type = MenuType.DeviceMenu,
            DataId = device.Id,
            Icon = SegoeFluentIcons.Devices4.Glyph,
        };
        menu.ParentId = deviceMainMenu.Id;
        var addDeviceMenuId = await db.Insertable<DbMenu>(menu.CopyTo<DbMenu>())
            .ExecuteReturnIdentityAsync();
        stopwatch.Stop();
        // Logger.Info($"添加设备菜单 '{device.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return addDeviceMenuId;
    }

    public async Task<int> Edit(MenuBean menu)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await db.Updateable<DbMenu>(menu.CopyTo<DbMenu>()).ExecuteCommandAsync();
            stopwatch.Stop();
            // Logger.Info($"编辑菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }
}