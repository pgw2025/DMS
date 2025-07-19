using AutoMapper;
using DMS.Core.Enums;
using DMS.Core.Helper;
using DMS.Core.Models;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using SqlSugar;
using System.Diagnostics;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services
{
    public class MenuService
    {
        private readonly IMapper _mapper;
        private readonly SqlSugarDbContext _dbContext;
        private SqlSugarClient Db => _dbContext.GetInstance();

        public MenuService(IMapper mapper, SqlSugarDbContext dbContext)
        {
            _mapper = mapper;
            _dbContext = dbContext;
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
                Type = MenuType.AddVariableTableMenu,
                ParentId = parentMenuId,
                DataId = dbDevice.Id
            };
            var addTableRes = await db.Insertable<DbMenu>(addVarTable)
                                      .ExecuteCommandAsync();
            stopwatch.Stop();
            return addTableRes;
        }

        public async Task<int> AddAsync(DbDevice device, SqlSugarClient db)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var deviceMainMenu = await db.Queryable<DbMenu>()
                                         .FirstAsync(m => m.Name == "设备");
            if (deviceMainMenu == null)
                throw new InvalidOperationException("没有找到设备菜单！！");

            MenuBean menu = new MenuBean()
            {
                Name = device.Name,
                Type = MenuType.DeviceMenu,
                DataId = device.Id,
            };
            menu.ParentId = deviceMainMenu.Id;
            var addDeviceMenuId = await db.Insertable<DbMenu>(_mapper.Map<DbMenu>(menu))
                                          .ExecuteReturnIdentityAsync();
            stopwatch.Stop();
            NlogHelper.Info($"添加设备菜单 '{device.Name}' 耗时：{stopwatch.ElapsedMilliseconds}ms");
            return addDeviceMenuId;
        }

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
    }
}
