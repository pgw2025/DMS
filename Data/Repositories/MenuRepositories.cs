using System.Windows.Controls;
using PMSWPF.Data.Entities;
using PMSWPF.Extensions;
using PMSWPF.Models;
using SqlSugar;

namespace PMSWPF.Data.Repositories;

public class MenuRepositories
{
    private readonly SqlSugarClient _db;

    public MenuRepositories()
    {
        _db=DbContext.GetInstance();
    }

    public async Task<List<MenuBean>> GetMenu()
    {
        // //无主键用法新:5.1.4.110
        // db.Queryable<Tree>().ToTree(it=>it.Child,it=>it.ParentId,0,it=>it.Id)//+4重载
        List<MenuBean> menus=new();
        var dbMenuList=await  _db.Queryable<DbMenu>().ToTreeAsync(dm=>dm.Items,dm=>dm.ParentId,0);
        foreach (var item in dbMenuList)
        {
            menus.Add(item.CopyTo<MenuBean>());
        }
        return menus;
    }
    
    public async Task<int> AddMenu(MenuBean menu)
    {
        return await _db.Insertable<DbMenu>(menu.CopyTo<DbMenu>()).ExecuteCommandAsync();
    }
}