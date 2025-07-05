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
            return await DeleteMenu(menu, db);
        }
    }

    public async Task<int> DeleteMenu(MenuBean menu, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var childList = await db.Queryable<DbMenu>()
                                .ToChildListAsync(it => it.ParentId, menu.Id);
        var result = await db.Deleteable<DbMenu>(childList)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        Logger.Info($"删除菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public async Task<List<MenuBean>> GetMenuTrees()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            List<MenuBean> menuTree = new();
            var dbMenuTree = await db.Queryable<DbMenu>()
                                     .ToTreeAsync(dm => dm.Items, dm => dm.ParentId, 0);

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
        using var db = DbContext.GetInstance();
        var result = await Add(menu, db);
        stopwatch.Stop();
        Logger.Info($"添加菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }


    /// <summary>
    /// 添加菜单，支持事务
    /// </summary>
    /// <param name="menu"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    public async Task<int> Add(MenuBean menu, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await db.Insertable<DbMenu>(menu.CopyTo<DbMenu>())
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        Logger.Info($"添加菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }


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
        var addTableRes = await db.Insertable<DbMenu>(addVarTable)
                                  .ExecuteCommandAsync();
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
    public async Task<int> Add(DbDevice device, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var deviceMainMenu = await db.Queryable<DbMenu>()
                                     .FirstAsync(m => m.Name == "设备");
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
        Logger.Info($"添加设备菜单 '{device.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return addDeviceMenuId;
    }

    /// <summary>
    /// 编辑菜单
    /// </summary>
    /// <param name="menu"></param>
    /// <returns></returns>
    public async Task<int> Edit(MenuBean menu)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await Edit(menu, db);
            stopwatch.Stop();
            Logger.Info($"编辑菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 编辑菜单,支持事务
    /// </summary>
    /// <param name="menu"></param>
    /// <returns></returns>
    public async Task<int> Edit(MenuBean menu, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await db.Updateable<DbMenu>(menu.CopyTo<DbMenu>())
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        Logger.Info($"编辑菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public async Task<MenuBean?> GetMenuByDataId(int dataId, MenuType menuType)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await db.Queryable<DbMenu>()
                                 .FirstAsync(m => m.DataId == dataId && m.Type == menuType);
            stopwatch.Stop();
            Logger.Info($"根据DataId '{dataId}' 和 MenuType '{menuType}' 获取菜单耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result?.CopyTo<MenuBean>();
        }
    }

    public async Task<MenuBean> GetMainMenuByName(string name)
    {
        using var db = DbContext.GetInstance();
       var dbMenu= await db.Queryable<DbMenu>().FirstAsync(m => m.Name == name  && m.Type == MenuType.MainMenu);
       return dbMenu.CopyTo<MenuBean>();
    }
}