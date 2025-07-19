using System.Diagnostics;
using SqlSugar;
using AutoMapper;
using DMS.Infrastructure.Entities;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Interfaces;

namespace DMS.Infrastructure.Repositories;

public class MenuRepository : BaseRepository<DbMenu>
{
    public MenuRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
    }

    public override async Task<int> DeleteAsync(DbMenu menu)
    {
        return await base.DeleteAsync(menu);
    }

    

    public async Task<List<DbMenu>> GetMenuTreesAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var dbMenuTree = await Db.Queryable<DbMenu>()
                                 .ToTreeAsync(dm => dm.Items, dm => dm.ParentId, 0);

        stopwatch.Stop();
        NlogHelper.Info($"获取菜单树耗时：{stopwatch.ElapsedMilliseconds}ms");
        return dbMenuTree;
    }


    public override async Task<DbMenu> AddAsync(DbMenu menu)
    {
        return await base.AddAsync(menu);
    }


    /// <summary>
    /// 添加菜单，支持事务
    /// </summary>
    /// <param name="menu"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    


    


    /// <summary>
    /// 添加设备菜单
    /// </summary>
    /// <param name="device"></param>
    /// <param name="db"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    

    /// <summary>
    /// 编辑菜单
    /// </summary>
    /// <param name="menu"></param>
    /// <returns></returns>
    public override async Task<int> UpdateAsync(DbMenu menu)
    {
        return await base.UpdateAsync(menu);
    }

    /// <summary>
    /// 编辑菜单,支持事务
    /// </summary>
    /// <param name="menu"></param>
    /// <returns></returns>
    

    public async Task<DbMenu?> GetMenuByDataIdAsync(int dataId, MenuType menuType)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Queryable<DbMenu>()
                             .FirstAsync(m => m.DataId == dataId && m.Type == menuType);
        stopwatch.Stop();
        NlogHelper.Info($"根据DataId '{dataId}' 和 MenuType '{menuType}' 获取菜单耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    public async Task<DbMenu> GetMainMenuByNameAsync(string name)
    {
       var dbMenu= await Db.Queryable<DbMenu>().FirstAsync(m => m.Name == name  && m.Type == MenuType.MainMenu);
       return dbMenu;
    }
}