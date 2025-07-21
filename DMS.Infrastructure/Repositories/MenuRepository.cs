using System.Diagnostics;
using AutoMapper;
using DMS.Core.Helper;
using DMS.Core.Interfaces.Repositories;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;

namespace DMS.Infrastructure.Repositories;

public class MenuRepository : BaseRepository<DbMenu>, IMenuRepository
{
    private readonly IMapper _mapper;

    public MenuRepository(IMapper mapper, SqlSugarDbContext dbContext)
        : base(dbContext)
    {
        _mapper = mapper;
    }


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


    public async Task<MenuBean> GetByIdAsync(int id)
    {
        var dbMenu = await base.GetByIdAsync(id);
        return _mapper.Map<MenuBean>(dbMenu);
    }

    public async Task<List<MenuBean>> GetAllAsync()
    {
        var dbList = await base.GetAllAsync();
        return _mapper.Map<List<MenuBean>>(dbList);
    }

    public async Task<MenuBean> AddAsync(MenuBean entity)
    {
        var dbMenu = await base.AddAsync(_mapper.Map<DbMenu>(entity));
        return _mapper.Map(dbMenu, entity);
    }

    public async Task<int> UpdateAsync(MenuBean entity) => await base.UpdateAsync(_mapper.Map<DbMenu>(entity));


    public async Task<int> DeleteAsync(MenuBean entity) => await base.DeleteAsync(_mapper.Map<DbMenu>(entity));

    public async Task<int> DeleteAsync(int id)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var result = await Db.Deleteable(new DbMenu { Id = id })
                             .ExecuteCommandAsync();
        stopwatch.Stop();
        NlogHelper.Info($"Delete {typeof(DbMenu)},ID={id},耗时：{stopwatch.ElapsedMilliseconds}ms");
        return result;
    }
}