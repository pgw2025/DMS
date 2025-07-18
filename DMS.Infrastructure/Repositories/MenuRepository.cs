using System.Diagnostics;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using DMS.Extensions;
using SqlSugar;
using AutoMapper;
using DMS.Infrastructure.Entities;
using DMS.Core.Enums;
using DMS.Helper;
using DMS.Models;

namespace DMS.Infrastructure.Repositories;

public class MenuRepository
{
    private readonly IMapper _mapper;

    public MenuRepository(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<int> DeleteAsync(MenuBean menu)
    {
        using var db = DbContext.GetInstance();
        return await DeleteAsync(menu, db);
    }

    public async Task<int> DeleteAsync(MenuBean menu, SqlSugarClient db)
    {
        
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var childList = await db.Queryable<DbMenu>()
                                .ToChildListAsync(it => it.ParentId, menu.Id);
        var result = await db.Deleteable<DbMenu>(childList)
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"删除菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public async Task<List<MenuBean>> GetMenuTreesAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            List<MenuBean> menuTree = new();
            var dbMenuTree = await db.Queryable<DbMenu>()
                                     .ToTreeAsync(dm => dm.Items, dm => dm.ParentId, 0);

            foreach (var dbMenu in dbMenuTree)
                menuTree.Add(_mapper.Map<MenuBean>(dbMenu));
            stopwatch.Stop();
            NlogHelper.Info($"获取菜单树耗时：{stopwatch.ElapsedMilliseconds}ms");
            return menuTree;
        }
    }


    public async Task<int> AddAsync(MenuBean menu)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using var db = DbContext.GetInstance();
        var result = await AddAsync(menu, db);
        stopwatch.Stop();
        NlogHelper.Info($"添加菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }


    /// <summary>
    /// 添加菜单，支持事务
    /// </summary>
    /// <param name="menu"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    public async Task<int> AddAsync(MenuBean menu, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await db.Insertable<DbMenu>(_mapper.Map<DbMenu>(menu))
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"添加菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }


    public async Task<int> AddVarTableMenuAsync(DbDevice dbDevice, int parentMenuId, SqlSugarClient db)
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
        // NlogHelper.Info($"添加变量表菜单 '{addVarTable.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return addTableRes;
    }


    /// <summary>
    /// 添加设备菜单
    /// </summary>
    /// <param name="device"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<int> AddAsync(DbDevice device, SqlSugarClient db)
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
        var addDeviceMenuId = await db.Insertable<DbMenu>(_mapper.Map<DbMenu>(menu))
                                      .ExecuteReturnIdentityAsync();
        stopwatch.Stop();
        NlogHelper.Info($"添加设备菜单 '{device.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return addDeviceMenuId;
    }

    /// <summary>
    /// 编辑菜单
    /// </summary>
    /// <param name="menu"></param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(MenuBean menu)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await UpdateAsync(menu, db);
            stopwatch.Stop();
            NlogHelper.Info($"编辑菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return result;
        }
    }

    /// <summary>
    /// 编辑菜单,支持事务
    /// </summary>
    /// <param name="menu"></param>
    /// <returns></returns>
    public async Task<int> UpdateAsync(MenuBean menu, SqlSugarClient db)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await db.Updateable<DbMenu>(_mapper.Map<DbMenu>(menu))
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"编辑菜单 '{menu.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public async Task<MenuBean?> GetMenuByDataIdAsync(int dataId, MenuType menuType)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        using (var db = DbContext.GetInstance())
        {
            var result = await db.Queryable<DbMenu>()
                                 .FirstAsync(m => m.DataId == dataId && m.Type == menuType);
            stopwatch.Stop();
            NlogHelper.Info($"根据DataId '{dataId}' 和 MenuType '{menuType}' 获取菜单耗时：{stopwatch.ElapsedMilliseconds}ms");
            return _mapper.Map<MenuBean>(result);
        }
    }

    public async Task<MenuBean> GetMainMenuByNameAsync(string name)
    {
        using var db = DbContext.GetInstance();
       var dbMenu= await db.Queryable<DbMenu>().FirstAsync(m => m.Name == name  && m.Type == MenuType.MainMenu);
       return _mapper.Map<MenuBean>(dbMenu);
    }
}