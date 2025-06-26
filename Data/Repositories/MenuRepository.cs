using System.Windows.Controls;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using PMSWPF.Data.Entities;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Helper;
using PMSWPF.Message;
using PMSWPF.Models;
using SqlSugar;

namespace PMSWPF.Data.Repositories;

public class MenuRepository
{
    private readonly SqlSugarClient _db;

    public MenuRepository()
    {
        _db = DbContext.GetInstance();
    }

    public async Task<List<MenuBean>> GetMenu()
    {
        // //无主键用法新:5.1.4.110
        // db.Queryable<Tree>().ToTree(it=>it.Child,it=>it.ParentId,0,it=>it.Id)//+4重载
        List<MenuBean> menus = new();
        var dbMenuList = await _db.Queryable<DbMenu>().ToTreeAsync(dm => dm.Items, dm => dm.ParentId, 0);
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


    public async Task<bool> AddDeviceMenu(MenuBean menu)
    {
        bool result = false;
        var deviceMainMenu = await _db.Queryable<DbMenu>().FirstAsync(m => m.Name == "设备");
        if (deviceMainMenu == null)
            throw new InvalidOperationException("没有找到设备菜单！！");

        menu.ParentId=deviceMainMenu.Id;
        var addDeviceMenuRes = await _db.Insertable<DbMenu>(menu.CopyTo<DbMenu>())
            .ExecuteCommandAsync();
        if (addDeviceMenuRes == 0)
            throw new InvalidOperationException($"{menu.Name},设备菜单添加失败！！");

        var addDM = await _db.Queryable<DbMenu>().OrderBy(m => m.Id, OrderByType.Desc)
            .FirstAsync(m => m.Name == menu.Name);
        if (addDM == null)
            throw new InvalidOperationException($"添加默认变量表菜单时，没有找到名字为：{menu.Name}的菜单项！");


        var defVarTable=new MenuBean()
        {
            Name = "默认变量表",
            Icon = SegoeFluentIcons.Tablet.Glyph,
            ParentId = addDM.Id,
        };
        var addVarTable=new MenuBean()
        {
            Name = "添加变量表",
            Icon = SegoeFluentIcons.Add.Glyph,
            ParentId = addDM.Id,
        };
        var defTableRes = await _db.Insertable<DbMenu>(defVarTable).ExecuteCommandAsync();
        var addTableRes = await _db.Insertable<DbMenu>(addVarTable).ExecuteCommandAsync();
        if ((addTableRes+defTableRes) != 2)
        {
            // 如果出错删除原来添加的设备菜单
            await _db.Deleteable<DbMenu>().Where(m=>m.Id==addDM.Id).ExecuteCommandAsync();
            throw new InvalidOperationException("添加默认变量表时发生了错误！！");
        }
            

        return true;
    }
}
