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
        List<DbMenu> dbMenuList = await _db.Queryable<DbMenu>().ToListAsync();

        List<MenuBean> menuTree = new();
        var dbMenuTree = await _db.Queryable<DbMenu>().ToTreeAsync(dm => dm.Items, dm => dm.ParentId, 0);
        foreach (var dbMenu in dbMenuTree)
        {
            AddParent(dbMenu);
            menuTree.Add(dbMenu.CopyTo<MenuBean>());
        }

        return menuTree;
    }

    private void AddParent(DbMenu dbMenu)
    {
        if (dbMenu.Items == null || dbMenu.Items.Count == 0)
            return;
        foreach (var item in dbMenu.Items)
        {
            item.Parent = dbMenu;
            if (item.Items!=null && item.Items.Count>0)
            {
                AddParent(item);
            }
            
        }
        
        
    }

    public async Task<int> AddMenu(MenuBean menu)
    {
        return await _db.Insertable<DbMenu>(menu.CopyTo<DbMenu>()).ExecuteCommandAsync();
    }


    public async Task<bool> AddDeviceMenu(Device device)
    {
        bool result = false;
        var deviceMainMenu = await _db.Queryable<DbMenu>().FirstAsync(m => m.Name == "设备");
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
        var addDeviceMenuId = await _db.Insertable<DbMenu>(menu.CopyTo<DbMenu>())
            .ExecuteReturnIdentityAsync();
        if (addDeviceMenuId == 0)
            throw new InvalidOperationException($"{menu.Name},设备菜单添加失败！！");

        var defVarTable = await _db.Queryable<DbVariableTable>()
            .FirstAsync(v => v.DeviceId == device.Id && v.Name == "默认变量表");
        if (defVarTable == null)
            throw new InvalidOperationException($"没有找到{device.Name}的默认变量表。");
        var defVarTableMenu = new MenuBean()
        {
            Name = "默认变量表",
            Icon = SegoeFluentIcons.Tablet.Glyph,
            Type = MenuType.VariableTableMenu,
            ParentId = addDeviceMenuId,
            DataId = defVarTable.Id
        };
        var addVarTable = new MenuBean()
        {
            Name = "添加变量表",
            Icon = SegoeFluentIcons.Add.Glyph,
            Type = MenuType.AddVariableTableMenu,
            ParentId = addDeviceMenuId,
        };
        var defTableRes = await _db.Insertable<DbMenu>(defVarTableMenu).ExecuteCommandAsync();
        var addTableRes = await _db.Insertable<DbMenu>(addVarTable).ExecuteCommandAsync();
        if ((addTableRes + defTableRes) != 2)
        {
            // 如果出错删除原来添加的设备菜单
            await _db.Deleteable<DbMenu>().Where(m => m.Id == addDeviceMenuId).ExecuteCommandAsync();
            throw new InvalidOperationException("添加默认变量表时发生了错误！！");
        }


        return true;
    }
}