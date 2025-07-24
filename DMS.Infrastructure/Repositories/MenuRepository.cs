using System.Diagnostics;
using AutoMapper;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

/// <summary>
/// 菜单仓储实现类，负责菜单数据的持久化操作。
/// 继承自 <see cref="BaseRepository{DbMenu}"/> 并实现 <see cref="IMenuRepository"/> 接口。
/// </summary>
public class MenuRepository : BaseRepository<DbMenu>, IMenuRepository
{
    private readonly IMapper _mapper;

    /// <summary>
    /// 构造函数，注入 AutoMapper 和 SqlSugarDbContext。
    /// </summary>
    /// <param name="mapper">AutoMapper 实例，用于实体模型和数据库模型之间的映射。</param>
    /// <param name="dbContext">SqlSugar 数据库上下文，用于数据库操作。</param>
    public MenuRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// 异步获取所有菜单树结构。
    /// </summary>
    /// <returns>包含所有菜单树结构的列表。</returns>
    public async Task<List<MenuBean>> GetMenuTreesAsync()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var dbMenuTree = await Db.Queryable<DbMenu>()
                                 .ToTreeAsync(dm => dm.Childrens, dm => dm.ParentId, 0);
        stopwatch.Stop();
        NlogHelper.Info($"获取菜单树耗时：{stopwatch.ElapsedMilliseconds}ms");
        return _mapper.Map<List<MenuBean>>(dbMenuTree);
    }

    /// <summary>
    /// 异步根据ID获取单个菜单。
    /// </summary>
    /// <param name="id">菜单的唯一标识符。</param>
    /// <returns>对应的菜单实体，如果不存在则为null。</returns>
    public async Task<MenuBean> GetByIdAsync(int id)
    {
        var dbMenu = await base.GetByIdAsync(id);
        return _mapper.Map<MenuBean>(dbMenu);
    }

    /// <summary>
    /// 异步获取所有菜单。
    /// </summary>
    /// <returns>包含所有菜单实体的列表。</returns>
    public async Task<List<MenuBean>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<MenuBean>>(dbList);
    }

    /// <summary>
    /// 异步添加新菜单。
    /// </summary>
    /// <param name="entity">要添加的菜单实体。</param>
    /// <returns>添加成功后的菜单实体（包含数据库生成的ID等信息）。</returns>
    public async Task<MenuBean> AddAsync(MenuBean entity)
    {
        var dbMenu = await base.AddAsync(_mapper.Map<DbMenu>(entity));
        return _mapper.Map(dbMenu, entity);
    }

    /// <summary>
    /// 异步更新现有菜单。
    /// </summary>
    /// <param name="entity">要更新的菜单实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> UpdateAsync(MenuBean entity) => await base.UpdateAsync(_mapper.Map<DbMenu>(entity));

    /// <summary>
    /// 异步删除菜单。
    /// </summary>
    /// <param name="entity">要删除的菜单实体。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteAsync(MenuBean entity) => await base.DeleteAsync(_mapper.Map<DbMenu>(entity));

    /// <summary>
    /// 异步根据ID删除菜单。
    /// </summary>
    /// <param name="id">要删除菜单的唯一标识符。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new DbMenu { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbMenu)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }

    /// <summary>
    /// 异步根据菜单ID删除菜单树（包括子菜单）。
    /// </summary>
    /// <param name="id">要删除菜单树的根菜单ID。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteMenuTreeByIdAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        int delConut = 0;
        var childList = await Db.Queryable<DbMenu>()
                                .ToChildListAsync(c => c.ParentId, id);
        delConut = await Db.Deleteable<DbMenu>(childList)
                           .ExecuteCommandAsync();
        delConut += await Db.Deleteable<DbMenu>()
                            .Where(m => m.Id == id)
                            .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbMenu)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return delConut;
    }

    /// <summary>
    /// 异步根据菜单类型和目标ID删除菜单树。
    /// </summary>
    /// <param name="menuType">菜单类型。</param>
    /// <param name="targetId">目标ID。</param>
    /// <returns>受影响的行数。</returns>
    public async Task<int> DeleteMenuTreeByTargetIdAsync(MenuType menuType, int targetId)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var menu = await Db.Queryable<DbMenu>().FirstAsync(m => m.MenuType == menuType && m.TargetId == targetId);
        if (menu == null) return 0;
        var childList = await Db.Queryable<DbMenu>()
            .ToChildListAsync(c => c.ParentId, menu.Id);
        var delConut = await Db.Deleteable<DbMenu>(childList)
            .ExecuteCommandAsync();
        delConut += await Db.Deleteable<DbMenu>()
            .Where(m => m.Id == menu.Id)
            .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbMenu)},TargetId={targetId},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return delConut;
    }

    /// <summary>
    /// 异步根据菜单类型和目标ID获取菜单。
    /// </summary>
    /// <param name="menuType">菜单类型。</param>
    /// <param name="targetId">目标ID。</param>
    /// <returns>对应的菜单实体，如果不存在则为null。</returns>
    public async Task<MenuBean> GetMenuByTargetIdAsync(MenuType menuType, int targetId)
    {
        var dbMenu = await Db.Queryable<DbMenu>().FirstAsync(m => m.MenuType == menuType && m.TargetId == targetId);
        return _mapper.Map<MenuBean>(dbMenu);
    }

    /// <summary>
    /// 异步获取指定数量的菜单。
    /// </summary>
    /// <param name="number">要获取的菜单数量。</param>
    /// <returns>包含指定数量菜单实体的列表。</returns>
    public new async Task<List<MenuBean>> TakeAsync(int number)
    {
        var dbList = await base.TakeAsync(number);
        return _mapper.Map<List<MenuBean>>(dbList);
    }
}