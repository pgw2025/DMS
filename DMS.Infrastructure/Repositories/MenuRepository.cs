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


    public async Task<MenuBean> GetByIdAsync(int id) => throw new NotImplementedException();

    public async Task<List<MenuBean>> GetAllAsync() => throw new NotImplementedException();

    public async Task<MenuBean> AddAsync(MenuBean entity) => throw new NotImplementedException();

    public async Task<int> UpdateAsync(MenuBean entity) => throw new NotImplementedException();

    public async Task<int> DeleteAsync(MenuBean entity) => throw new NotImplementedException();
}