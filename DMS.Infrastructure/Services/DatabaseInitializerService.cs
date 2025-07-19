using DMS.Config;
using DMS.Core.Enums;
using DMS.Infrastructure.Data;
using DMS.Infrastructure.Entities;
using SqlSugar;
using System;
using System.Threading.Tasks;

namespace DMS.Infrastructure.Services
{
    public class DatabaseInitializerService : DMS.Infrastructure.Interfaces.IDatabaseService
    {
        private readonly SqlSugarClient _db;

        public DatabaseInitializerService(SqlSugarDbContext dbContext)
        {
            _db = dbContext.GetInstance();
        }

        public void InitializeDataBase()
        {
            _db.DbMaintenance.CreateDatabase();
            _db.CodeFirst.InitTables<DbNlog>();
            _db.CodeFirst.InitTables<DbDevice>();
            _db.CodeFirst.InitTables<DbVariableTable>();
            _db.CodeFirst.InitTables<DbVariable>();
            _db.CodeFirst.InitTables<DbVariableHistory>();
            _db.CodeFirst.InitTables<DbUser>();
            _db.CodeFirst.InitTables<DbMqtt>();
            _db.CodeFirst.InitTables<DbVariableMqtt>();
            _db.CodeFirst.InitTables<DbMenu>();
        }

        public async Task InitializeMenu()
        {
            var homeMenu = new DbMenu()
            { Name = "主页", Type = MenuType.MainMenu, Icon = "Home", ParentId = 0 }; // Icon needs to be adjusted if it's not a string

            var deviceMenu = new DbMenu()
            {
                Name = "设备", Type = MenuType.MainMenu, Icon = "Devices3",
                ParentId = 0
            };
            var dataTransfromMenu = new DbMenu()
            {
                Name = "数据转换", Type = MenuType.MainMenu,
                Icon = "ChromeSwitch", ParentId = 0
            };
            var mqttMenu = new DbMenu()
            {
                Name = "Mqtt服务器", Type = MenuType.MainMenu, Icon = "Cloud",
                ParentId = 0
            };

            var settingMenu = new DbMenu()
            {
                Name = "设置", Type = MenuType.MainMenu, Icon = "Settings",
                ParentId = 0
            };
            var aboutMenu = new DbMenu()
            { Name = "关于", Type = MenuType.MainMenu, Icon = "Info", ParentId = 0 };

            await CheckMainMenuExist(homeMenu);
            await CheckMainMenuExist(deviceMenu);
            await CheckMainMenuExist(dataTransfromMenu);
            await CheckMainMenuExist(mqttMenu);
            await CheckMainMenuExist(settingMenu);
            await CheckMainMenuExist(aboutMenu);
        }

        private async Task CheckMainMenuExist(DbMenu menu)
        {
            var homeMenuExist = await _db.Queryable<DbMenu>()
                                        .FirstAsync(dm => dm.Name == menu.Name);
            if (homeMenuExist == null)
            {
                await _db.Insertable<DbMenu>(menu)
                        .ExecuteCommandAsync();
            }
        }
    }
}