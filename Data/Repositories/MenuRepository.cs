using iNKORE.UI.WPF.Modern.Common.IconKeys;
using PMSWPF.Data.Entities;
using PMSWPF.Enums;
using PMSWPF.Extensions;
using PMSWPF.Models;

namespace PMSWPF.Data.Repositories;

public class MenuRepository
{
    public MenuRepository()
    {
    }

    public async Task<int> DeleteMenu(MenuBean menu)
    {
        using (var db = DbContext.GetInstance())
        {
            var childList = await db.Queryable<DbMenu>().ToChildListAsync(it => it.ParentId, menu.Id);
            return await db.Deleteable<DbMenu>(childList).ExecuteCommandAsync();
        }
    }

    public async Task<List<MenuBean>> GetMenuTrees()
    {
        // //无主键用法新:5.1.4.110
        // db.Queryable<Tree>().ToTree(it=>it.Child,it=>it.ParentId,0,it=>it.Id)//+4重载
        // List<DbMenu> dbMenuList = await _db.Queryable<DbMenu>().ToListAsync();

        using (var db = DbContext.GetInstance())
        {
            List<MenuBean> menuTree = new();
            var dbMenuTree = await db.Queryable<DbMenu>().ToTreeAsync(dm => dm.Items, dm => dm.ParentId, 0);

            foreach (var dbMenu in dbMenuTree)
                menuTree.Add(dbMenu.CopyTo<MenuBean>());

            return menuTree;
        }
    }


    public async Task<int> AddMenu(MenuBean menu)
    {
        using (var db = DbContext.GetInstance())
        {
            return await db.Insertable<DbMenu>(menu.CopyTo<DbMenu>()).ExecuteCommandAsync();
        }
    }


    public async Task<bool> AddDeviceMenu(Device device)
    {
        using (var db = DbContext.GetInstance())
        {
            bool result = false;
            var deviceMainMenu = await db.Queryable<DbMenu>().FirstAsync(m => m.Name == "设备");
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
            if (addDeviceMenuId == 0)
                throw new InvalidOperationException($"{menu.Name},设备菜单添加失败！！");

            var defVarTable = await db.Queryable<DbVariableTable>()
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
            var defTableRes = await db.Insertable<DbMenu>(defVarTableMenu).ExecuteCommandAsync();
            var addTableRes = await db.Insertable<DbMenu>(addVarTable).ExecuteCommandAsync();
            if ((addTableRes + defTableRes) != 2)
            {
                // 如果出错删除原来添加的设备菜单
                await db.Deleteable<DbMenu>().Where(m => m.Id == addDeviceMenuId).ExecuteCommandAsync();
                throw new InvalidOperationException("添加默认变量表时发生了错误！！");
            }


            return true;
        }
    }

    public async Task<int> Edit(MenuBean menu)
    {
        using (var db = DbContext.GetInstance())
        {
            return await db.Updateable<DbMenu>(menu.CopyTo<DbMenu>()).ExecuteCommandAsync();
        }
    }
}