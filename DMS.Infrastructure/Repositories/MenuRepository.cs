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

public class MenuRepository : BaseRepository<DbMenu>,IMenuRepository
{
    public MenuRepository(SqlSugarDbContext dbContext)
        : base(dbContext)
    {
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

}